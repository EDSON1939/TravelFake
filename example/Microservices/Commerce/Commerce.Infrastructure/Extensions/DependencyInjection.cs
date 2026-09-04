using Commerce.Domain.Interfaces;
using Commerce.Domain.Repositories;
using Commerce.Infrastructure.ExternalServices;
using Commerce.Infrastructure.Grpc;
using Commerce.Infrastructure.Repositories;
using Core.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDependence(configuration);
        services.AddScoped<ICommerceRepository, CommerceRepository>();

        services.AddGrpcClient<AccountsAndMovements.AccountsAndMovementsClient>(
            nameof(AccountsAndMovements), configuration);
        services.AddScoped<AccountsAndMovementsGrpcClient>();
        services.AddScoped<IAccountsAndMovementsService, AccountsAndMovementsService>();

        return services;
    }
}
