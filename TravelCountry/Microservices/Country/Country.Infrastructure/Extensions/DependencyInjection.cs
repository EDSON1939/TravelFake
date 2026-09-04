using Core.Infrastructure.Extensions;
using Country.Domain.Repositories;
using Country.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Country.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDependence(configuration);
        services.AddScoped<ICountryRepository, CountryRepository>();
        return services;
    }
}
