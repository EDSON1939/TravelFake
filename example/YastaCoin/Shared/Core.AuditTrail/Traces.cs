using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog.Core;

namespace Core.AuditTrail
{
    public class TracingSetup
    {
        public static void Init(WebApplicationBuilder builder, Logger logger)
        {
            var configuration = builder.Configuration.GetSection("OpenTelemetry");
            if (!configuration.Exists() || !configuration.GetValue<bool>("Enabled") || configuration["Endpoint"] == null)
            {
                logger.Warning("Open Telemetry Tracing is disabled, or no endpoint is configured");
                return;
            }
            var serviceName = configuration["ServiceName"] ?? string.Empty;
            builder.Services.AddOpenTelemetry().WithTracing(x => x.AddSource(serviceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddAspNetCoreInstrumentation(x => {
                    x.Filter = y => !y.Request.Path.Value?.Contains("/metrics") ?? false;
                    x.EnrichWithException = (activity, exception) => activity.SetTag("stackTrace", exception.StackTrace);
                })
                .AddHttpClientInstrumentation(x =>
                {
                    x.FilterHttpRequestMessage = y => !y.RequestUri?.ToString().Contains("/loki/api/v1/push") ?? false;
                    x.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) => activity.SetTag("http.request", httpRequestMessage?.Content?.ReadAsStringAsync().Result);
                    x.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) => activity.SetTag("http.response", httpResponseMessage.Content.ReadAsStringAsync().Result);
                    x.EnrichWithException = (activity, exception) => activity.SetTag("stackTrace", exception.StackTrace);
                })
                .AddSqlClientInstrumentation(x => x.SetDbStatementForText = true)
                .AddOtlpExporter(x => x.Endpoint = new Uri(configuration["Endpoint"] ?? string.Empty)));
        }
    }
}
