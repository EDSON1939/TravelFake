using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Core.Infrastructure.Http.Factory
{
    public class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) : DelegatingHandler
    {
        private const string MessageTemplate = "{TraceId} {RequestMethod} responded {StatusCode} in {Elapsed:0.0000} ms. Request: {Request} Response: {Response}";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                stopwatch.Stop();
                logger.LogInformation(MessageTemplate, Activity.Current?.TraceId, request.RequestUri?.AbsoluteUri, response.StatusCode, stopwatch.Elapsed.TotalMilliseconds,
                    request.Content?.ReadAsStringAsync(cancellationToken).Result, response.Content.ReadAsStringAsync(cancellationToken).Result);
                return response;
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                logger.LogError(MessageTemplate, Activity.Current?.TraceId, request.RequestUri?.AbsoluteUri, string.Empty, stopwatch.Elapsed.TotalMilliseconds,
                    request.Content?.ReadAsStringAsync(cancellationToken).Result, exception);
                throw;
            }
        }
    }
}
