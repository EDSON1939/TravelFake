using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Core.WorkerPool.Extensions
{
    public static class BuilderDependence
    {
        public static IHostBuilder AddBuilderDependence(this IHostBuilder host)
        {
            host
                .ConfigureAppConfiguration((x, y) =>
                {
                    y.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    y.AddEnvironmentVariables();
                })
                .UseSerilog()
                .ConfigureLogging((x, y) =>
                {
                    var serilog = new LoggerConfiguration().ReadFrom.Configuration(x.Configuration).CreateLogger();
                    y.AddSerilog(serilog);
                    y.ClearProviders();
                    y.AddSerilog();
                })
                .UseWindowsService();
            return host;
        }
    }
}
