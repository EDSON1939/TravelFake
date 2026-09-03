using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace Core.Infrastructure.Extensions
{
    /// <summary>
    /// Circuit breaker para clientes HTTP y gRPC. Reemplaza al
    /// Http/Factory/Policies/CircuitBreakerBuilder (Polly v7, fuera de uso).
    ///
    /// Corta las llamadas a un servicio caído en vez de esperar el timeout de
    /// conexión en cada request: con el destino apagado, la llamada pasa de
    /// segundos a milisegundos y deja de ocupar hilos.
    /// </summary>
    public static class CircuitBreakerDependence
    {
        private const int DefaultMinimumThroughput = 4;
        private const int DefaultSamplingSeconds   = 30;
        private const int DefaultBreakSeconds      = 15;

        /// <summary>
        /// Engancha el breaker al cliente. <paramref name="configuration"/> es la
        /// sección CircuitBreaker del servicio destino:
        ///
        ///   "CircuitBreaker": {
        ///     "MinimumThroughput": 4,   // llamadas mínimas en la ventana para opinar
        ///     "SamplingSeconds":  30,   // ancho de la ventana deslizante
        ///     "BreakSeconds":     15    // cuánto queda abierto antes de probar
        ///   }
        ///
        /// Ojo con gRPC: el breaker sólo ve fallos de transporte (conexión
        /// rechazada, DNS, TLS). Un error de negocio de gRPC viaja sobre HTTP 200
        /// y para esta capa es un éxito. Para eso está el retry nativo del canal,
        /// que sí lee grpc-status. Sobre HTTP normal, en cambio, también reacciona
        /// a los 5xx y 408.
        /// </summary>
        public static IHttpClientBuilder AddCircuitBreakerDependence(
            this IHttpClientBuilder builder, string service, IConfiguration configuration)
        {
            var failures = configuration.GetValue<int?>("MinimumThroughput") ?? DefaultMinimumThroughput;
            var sampling = configuration.GetValue<int?>("SamplingSeconds")   ?? DefaultSamplingSeconds;
            var breakFor = configuration.GetValue<int?>("BreakSeconds")      ?? DefaultBreakSeconds;

            builder.AddResilienceHandler("circuit-breaker", (pipeline, context) =>
            {
                var logger = context.ServiceProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("CircuitBreaker." + service);

                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    // 100% de fallos sobre MinimumThroughput llamadas dentro de la
                    // ventana = abrir. Con FailureRatio 1.0 equivale a "N seguidas":
                    // un solo éxito en la ventana ya impide la apertura.
                    FailureRatio      = 1.0,
                    MinimumThroughput = failures,
                    SamplingDuration  = TimeSpan.FromSeconds(sampling),
                    BreakDuration     = TimeSpan.FromSeconds(breakFor),

                    OnOpened = args =>
                    {
                        logger.LogWarning("Circuito ABIERTO hacia {Service} por {Seconds}s tras {Failures} fallos.",
                            service, args.BreakDuration.TotalSeconds, failures);
                        return default;
                    },
                    OnHalfOpened = _ =>
                    {
                        logger.LogInformation("Circuito SEMIABIERTO hacia {Service}: probando una llamada.", service);
                        return default;
                    },
                    OnClosed = _ =>
                    {
                        logger.LogInformation("Circuito CERRADO hacia {Service}: el servicio respondió.", service);
                        return default;
                    }
                });
            });

            return builder;
        }
    }
}
