using Core.Infrastructure.Database.Commands;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries;
using Core.Infrastructure.Database.Queries.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Extensions
{
    public static class DatabaseDependence
    {
        public static IServiceCollection AddDatabaseDependence(this IServiceCollection services, IConfiguration configuration, string connectionStringLabel = "")
        {
            if (!string.IsNullOrEmpty(connectionStringLabel))
            {
                services.AddKeyedScoped<ICommandContext, CommandContext>(connectionStringLabel, (x, y) =>
                    new CommandContext(configuration.GetValue<string>("Database:" + connectionStringLabel) ?? string.Empty));
                services.AddKeyedScoped<ICommandConnection, KeyedCommandConnection>(connectionStringLabel, (x, y) => new KeyedCommandConnection(x, connectionStringLabel));
                services.AddKeyedScoped<ICommand, KeyedCommand>(connectionStringLabel, (x, y) => new KeyedCommand(x, connectionStringLabel));
                services.AddKeyedScoped<IQueryContext, QueryContext>(connectionStringLabel, (x, y) =>
                    new QueryContext(configuration.GetValue<string>("Database:" + connectionStringLabel) ?? string.Empty));
                services.AddKeyedScoped<IQueryConnection, KeyedQueryConnection>(connectionStringLabel, (x, y) => new KeyedQueryConnection(x, connectionStringLabel));
                services.AddKeyedScoped<IQuery, KeyedQuery>(connectionStringLabel, (x, y) => new KeyedQuery(x, connectionStringLabel));
                return services;
            }
            else
            {
                services.AddScoped<ICommandContext, CommandContext>(x => new CommandContext(configuration.GetValue<string>("Database:ConnectionString") ?? string.Empty));
                services.AddScoped<ICommandConnection, CommandConnection>();
                services.AddScoped<ICommand, Command>();
                services.AddScoped<IQueryContext, QueryContext>(x => new QueryContext(configuration.GetValue<string>("Database:ConnectionString") ?? string.Empty));
                services.AddScoped<IQueryConnection, QueryConnection>();
                services.AddScoped<IQuery, Query>();
                return services;
            }
        }
    }
}
