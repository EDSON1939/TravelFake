using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qr.Application.Behaviors;
using Qr.Infrastructure.Extensions;

namespace Qr.Application.Extensions;

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