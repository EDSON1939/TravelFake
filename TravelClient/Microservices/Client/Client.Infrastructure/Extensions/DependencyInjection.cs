using Client.Domain.Repositories;
using Client.Domain.Services;
using Client.Infrastructure.Repositories;
using Client.Infrastructure.Services;
using Core.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDependence(configuration);
        services.AddScoped<IClientRepository, ClientRepository>();

        // ── Canal gRPC hacia el microservicio TravelCountry ──────────────────
        // Lee BaseAddress (y PolicyConfiguration si existe) de Connections:Country.
        services.AddGrpcClientDependence<global::Client.Infrastructure.Grpc.Country.CountryClient>(
            nameof(global::Client.Infrastructure.Grpc.Country.CountryClient),
            configuration.GetSection("Connections:Country"));

        services.AddScoped<ICountryService, CountryService>();

        return services;
    }
}
