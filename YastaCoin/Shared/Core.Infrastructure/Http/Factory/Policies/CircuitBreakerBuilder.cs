// =============================================================================
// FUERA DE USO — reemplazado por Core.Infrastructure.Extensions.CircuitBreakerDependence
//
// Se conserva comentado como referencia del camino HTTP/Polly v7. No volver a
// engancharlo sin arreglar antes estos tres puntos:
//
//   1. OnHttpBreak lanza una excepción genérica: pisa el error real, así que el
//      llamador nunca se entera de POR QUÉ se cayó el servicio.
//   2. Console.WriteLine no llega a Serilog ni a Loki: las transiciones del
//      circuito quedan invisibles en producción.
//   3. Espera las claves RetryCount y BreakDuration, que la sección
//      PolicyConfiguration de los appsettings no tiene. BreakDuration bindea a 0
//      y el circuito abre y pasa a semiabierto en el mismo instante: no protege.
//
// Además depende de Polly.Extensions.Http 3.0.0, que NuGet marca como legacy y
// cuya alternativa oficial es Microsoft.Extensions.Http.Resilience (Polly v8),
// que es lo que usa el reemplazo.
// =============================================================================

//using Polly;
//using Polly.CircuitBreaker;

//namespace Core.Infrastructure.Http.Factory.Policies
//{
//    public static class CircuitBreakerBuilder
//    {
//        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> Get(ICircuitBreaker configuration)
//        {
//            return HttpPolicyBuilder.GetBase()
//                .CircuitBreakerAsync(configuration.RetryCount + 1, TimeSpan.FromSeconds(configuration.BreakDuration),
//                    (result, breakDuration) => OnHttpBreak(result, breakDuration, configuration.RetryCount), OnHttpReset);
//        }

//        private static void OnHttpBreak(DelegateResult<HttpResponseMessage> result, TimeSpan breakDuration, int retryCount)
//        {
//            Console.WriteLine($"Service shutdown during {breakDuration} after {retryCount} failed retries.");
//            throw new Exception("Service inoperative. Please try again later.", result.Exception);
//        }

//        private static void OnHttpReset()
//        {
//            Console.WriteLine("Service restarted.");
//        }
//    }
//}
