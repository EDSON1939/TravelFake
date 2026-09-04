using Core.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qr.Domain.Interfaces;
using Qr.Domain.Repositories;
using Qr.Infrastructure.ExternalServices;
using Qr.Infrastructure.Grpc;
using Qr.Infrastructure.Repositories;

namespace Qr.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDependence(configuration);
        services.AddScoped<IQrRepository, QrRepository>();

        services.AddGrpcClient<Commerce.CommerceClient>(
            nameof(Commerce), configuration);
        services.AddScoped<CommerceGrpcClient>();
        services.AddScoped<ICommerceService, CommerceService>();

        return services;
    }
}