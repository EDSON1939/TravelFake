using Polly;
using System.Net;
using Polly.Retry;
using Polly.Contrib.WaitAndRetry;

namespace Core.Infrastructure.Http.Factory.Policies
{
    public static class HttpRetryPolicyBuilder
    {
        public static AsyncRetryPolicy<HttpResponseMessage> Get(IRetry configuration)
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(configuration.MedianFirstRetryDelay), configuration.RetryCount);
            return HttpPolicyBuilder.GetBase()
                .OrResult(x => x.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(delay, OnHttpRetry);
        }

        private static void OnHttpRetry(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount, Context context)
        {
            if (result.Result != null)
            {
                Console.WriteLine($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
            }
            else
            {
                Console.WriteLine($"Request failed because network failure. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
            }
        }
    }
}
