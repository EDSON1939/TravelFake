using Coin.Application.Behaviors;
using Coin.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Coin.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureDependence(configuration);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Only registers the validators of this assembly (does not scan the whole AppDomain)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
