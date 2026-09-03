using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Extensions
{
    public static class GrpcClientDependence
    {
        public static IHttpClientBuilder AddGrpcClientDependence<T>(this IServiceCollection services, string name, IConfiguration configuration, string port = "") where T : class
        {
            var uri = string.IsNullOrEmpty(port) ? configuration.GetValue<string>("BaseAddress") : (configuration.GetValue<string>("BaseAddress") + ":" + port);
            var client = services.AddGrpcClient<T>(name, x => x.Address = new Uri(uri ?? string.Empty));
            client.ConfigurePrimaryHttpMessageHandler(_ =>
                    new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
            if (configuration.GetSection("PolicyConfiguration").Exists())
            {
                client.ConfigureChannel(x => x = new GrpcChannelOptions
                {
                    ServiceConfig = new ServiceConfig
                    {
                        MethodConfigs = {
                            new MethodConfig
                            {
                                Names = { MethodName.Default },
                                RetryPolicy = new RetryPolicy
                                {
                                    MaxAttempts = configuration.GetValue<int>("PolicyConfiguration:RetryCount"),
                                    InitialBackoff = TimeSpan.FromSeconds(configuration.GetValue<int>("PolicyConfiguration:InitialBackoff")),
                                    MaxBackoff = TimeSpan.FromSeconds(configuration.GetValue<int>("PolicyConfiguration:MaxBackoff")),
                                    BackoffMultiplier = configuration.GetValue<int>("PolicyConfiguration:BackoffMultiplier"),
                                    RetryableStatusCodes = { StatusCode.Unavailable }
                                }
                            }
                        }
                    }
                });
            }
            return client;
        }
    }
}
