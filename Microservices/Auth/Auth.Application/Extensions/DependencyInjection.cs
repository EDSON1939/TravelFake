using Auth.Application.Behaviors;
using Auth.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application.Extensions;

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

        // Registra solo los validators de este assembly (no escanea todo el AppDomain)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
