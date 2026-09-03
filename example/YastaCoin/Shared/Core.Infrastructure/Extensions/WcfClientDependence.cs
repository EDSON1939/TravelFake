using Core.Infrastructure.Wcf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Core.Infrastructure.Extensions
{
    public static class WcfClientDependence
    {
        public static IServiceCollection AddWcfClientDependence<TInterface, TClient>(
        this IServiceCollection services,
        string configurationSection,
        IConfiguration configuration,
        Action<TClient, IConfiguration>? clientConfiguration=null,
        Action<Binding>? bindingConfiguration = null)
        where TInterface : class
        where TClient : ClientBase<TInterface>, TInterface, new()
        {
            return services.AddScoped<TInterface>(serviceProvider =>
            {
                var client = new TClient();
                var logger = serviceProvider.GetService<ILogger<TClient>>();
                ConfigureBasicSettings<TInterface, TClient>(client, configurationSection, configuration);
                if (clientConfiguration != null)
                    clientConfiguration(client, configuration);
                bindingConfiguration?.Invoke(client.Endpoint.Binding);
                if (logger != null)
                {
                    client.Endpoint.EndpointBehaviors.Add(new WcfLoggingBehavior(logger));
                }
                return client;
            });
        }

        private static void ConfigureBasicSettings<TInterface, TClient>(
       TClient client,
       string configurationSection,
       IConfiguration configuration)
       where TInterface : class
       where TClient : ClientBase<TInterface>, TInterface
        {
            var baseAddress = configuration.GetValue<string>($"{configurationSection}:BaseAddress");
            if (!string.IsNullOrEmpty(baseAddress))
            {
                client.Endpoint.Address = new EndpointAddress(baseAddress);
            }
            var timeout = configuration.GetValue<int>($"{configurationSection}:Timeout");
                var timeSpan = TimeSpan.FromSeconds(timeout);
                client.Endpoint.Binding.OpenTimeout = timeSpan;
                client.Endpoint.Binding.SendTimeout = timeSpan;
                client.Endpoint.Binding.CloseTimeout = timeSpan;
                client.Endpoint.Binding.ReceiveTimeout = timeSpan;
            
        }
    }
}
