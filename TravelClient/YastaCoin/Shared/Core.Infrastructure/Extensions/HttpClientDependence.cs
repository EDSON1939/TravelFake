using Core.Infrastructure.Http.Factory;
using Core.Infrastructure.Http.Factory.Policies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Extensions
{
    public static class HttpClientDependence
    {
        public static IHttpClientBuilder AddHttpClientDependence<T, U>(this IServiceCollection services, string name, string policyConfigurationName, IConfiguration configuration) where T : class where U : class, T
        {
            services.AddTransient<LoggingDelegatingHandler>();
            return services.AddScoped<T, U>().AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("BaseAddress") ?? string.Empty);
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("Timeout"));
            })
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(configuration.GetValue<int>("Lifetime")))
                .AddPolicyHandlers(policyConfigurationName, configuration);
        }
    }
}
