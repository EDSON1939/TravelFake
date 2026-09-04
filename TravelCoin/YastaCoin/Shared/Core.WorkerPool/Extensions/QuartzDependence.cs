using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Core.WorkerPool.Extensions
{
    public static class QuartzDependence
    {
        public static IServiceCollection AddQuartzDependence(this IServiceCollection services, IConfiguration configuration, List<(string Name, Type Type)> tasks)
        {
            foreach (var task in tasks)
            {
                if (configuration.GetValue<bool>($"Tasks:{task.Name}:Enabled"))
                {
                    services.AddQuartz(x =>
                    {
                        var jobKey = new JobKey(task.Name);
                        x.AddJob(task.Type, jobKey, opts => opts.WithIdentity(jobKey));
                        x.AddTrigger(opts => opts
                            .ForJob(jobKey)
                            .WithIdentity(task.Name)
                            .WithCronSchedule(configuration.GetValue<string>($"Tasks:{task.Name}:CronSchedule") ?? string.Empty));
                    });
                }
            }
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            return services;
        }
    }
}
