using Coin.Domain.Repositories;
using Coin.Infrastructure.Repositories;
using Core.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Coin.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDependence(configuration);
        services.AddScoped<ICoinRepository, CoinRepository>();
        return services;
    }
}
