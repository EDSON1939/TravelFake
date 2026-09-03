using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Http.Factory.Policies
{
    public static class HttpClientBuilder
    {
        public static IHttpClientBuilder AddPolicyHandlers(this IHttpClientBuilder httpClientBuilder, string policySectionName, IConfiguration configuration)
        {
            var policyConfig = new ConnectionConfig();
            configuration.Bind(policySectionName, policyConfig);

            // El circuit breaker ya no se engancha acá: vive en
            // Extensions/CircuitBreakerDependence.cs, con Polly v8 y activado por
            // la sección CircuitBreaker del servicio. Ver CircuitBreakerBuilder.cs.
            return httpClientBuilder.AddRetryPolicyHandler(policyConfig);
        }

        public static IHttpClientBuilder AddRetryPolicyHandler(this IHttpClientBuilder httpClientBuilder, IRetry configuration)
            => httpClientBuilder.AddPolicyHandler(HttpRetryPolicyBuilder.Get(configuration));

        //public static IHttpClientBuilder AddCircuitBreakerHandler(this IHttpClientBuilder httpClientBuilder, ICircuitBreaker configuration)
        //    => httpClientBuilder.AddPolicyHandler(CircuitBreakerBuilder.Get(configuration));
    }
}
