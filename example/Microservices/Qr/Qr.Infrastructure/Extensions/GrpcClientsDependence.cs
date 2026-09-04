using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Qr.Infrastructure.Extensions;

public static class GrpcClientsDependence
{
    public static IHttpClientBuilder AddGrpcClient<T>(
        this IServiceCollection services, string name, IConfiguration configuration) where T : class
    {
        // Prioridad: GrpcClients:<name>:Address → GrpcClients:<name> → BaseAddress
        var address = configuration.GetValue<string>($"GrpcClients:{name}:Address")
                      ?? configuration.GetValue<string>($"GrpcClients:{name}")
                      ?? configuration.GetValue<string>("BaseAddress")
                      ?? string.Empty;

        var client = services.AddGrpcClient<T>(name, x => x.Address = new Uri(address));
        client.ConfigurePrimaryHttpMessageHandler(_ =>
            new HttpClientHandler { ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });

        if (configuration.GetSection("PolicyConfiguration").Exists())
        {
            client.ConfigureChannel(x => x = new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig
                {
                    MethodConfigs =
                    {
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