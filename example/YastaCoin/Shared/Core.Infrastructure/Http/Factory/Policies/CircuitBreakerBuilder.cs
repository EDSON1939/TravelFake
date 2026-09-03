using Polly;
using Polly.CircuitBreaker;

namespace Core.Infrastructure.Http.Factory.Policies
{
    public static class CircuitBreakerBuilder
    {
        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> Get(ICircuitBreaker configuration)
        {
            return HttpPolicyBuilder.GetBase()
                .CircuitBreakerAsync(configuration.RetryCount + 1, TimeSpan.FromSeconds(configuration.BreakDuration),
                    (result, breakDuration) => OnHttpBreak(result, breakDuration, configuration.RetryCount), OnHttpReset);
        }

        private static void OnHttpBreak(DelegateResult<HttpResponseMessage> result, TimeSpan breakDuration, int retryCount)
        {
            Console.WriteLine($"Service shutdown during {breakDuration} after {retryCount} failed retries.");
            throw new Exception("Service inoperative. Please try again later.", result.Exception);
        }

        private static void OnHttpReset()
        {
            Console.WriteLine("Service restarted.");
        }
    }
}
