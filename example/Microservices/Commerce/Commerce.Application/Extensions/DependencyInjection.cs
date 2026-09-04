using Commerce.Application.Behaviors;
using Commerce.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Application.Extensions;

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

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
