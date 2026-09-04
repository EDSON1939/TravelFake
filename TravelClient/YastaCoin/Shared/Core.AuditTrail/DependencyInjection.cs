 using Core.AuditTrail.Grpc;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Builder;
using Serilog.Core;

namespace Core.AuditTrail
{
    public static class DependencyInjection
    {
        public static Logger AddMonitoringPlatform(this WebApplicationBuilder builder)
        {
            var logger = LoggerSetup.Init(builder);
            TracingSetup.Init(builder, logger);
            MetricsSetup.Init(builder, logger);
            return logger;
        }

        public static GrpcServiceOptions AddLogInterceptor(this GrpcServiceOptions options, Logger logger)
        {
            options.Interceptors.Add<LoggerInterceptor>(logger);
            return options;
        }
    }
}
