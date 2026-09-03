using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using Serilog.Core;

namespace Core.AuditTrail
{
    public static class MetricsSetup
    {
        private static IConfigurationSection? configuration;

        public static void Init(WebApplicationBuilder builder, Logger logger)
        {
            configuration = builder.Configuration.GetSection("OpenTelemetry");
            if (!configuration.Exists() || !configuration.GetValue<bool>("Enabled"))
            {
                logger.Warning("Open Telemetry Metrics are disabled");
                return;
            }

            builder.Services.AddOpenTelemetry().WithMetrics(x =>
                x.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddPrometheusExporter());
        }

        public static void UseMonitoringPlatform(this WebApplication app)
        {
            if (!configuration.Exists() || !configuration.GetValue<bool>("Enabled") || configuration["Endpoint"] == null)
            {
                return;
            }
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }
    }
}
