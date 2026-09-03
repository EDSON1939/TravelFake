using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using AccountsAndMovements.Infrastructure.ExternalServices;
using AccountsAndMovements.Infrastructure.Repositories;
using Core.Infrastructure.Extensions;
using Core.Infrastructure.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountsAndMovements.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDependence(configuration);
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();

        // Reenvia el JWT recibido hacia el siguiente salto de la cadena.
        services.AddTransient<TokenPropagationHandler>();

        // Los cuatro servicios que valida el pago. El retry y el circuit breaker
        // se enganchan solos si el appsettings del destino declara las secciones
        // PolicyConfiguration y CircuitBreaker.
        services.AddGrpcClientDependence<Client.Api.Grpc.Client.ClientClient>(
            "ClientClient", configuration.GetSection("Services:Client"))
            .AddHttpMessageHandler<TokenPropagationHandler>();
        services.AddGrpcClientDependence<Merchant.Api.Grpc.Merchant.MerchantClient>(
            "MerchantClient", configuration.GetSection("Services:Merchant"))
            .AddHttpMessageHandler<TokenPropagationHandler>();
        services.AddGrpcClientDependence<Qr.Api.Grpc.Qr.QrClient>(
            "QrClient", configuration.GetSection("Services:Qr"))
            .AddHttpMessageHandler<TokenPropagationHandler>();
        services.AddGrpcClientDependence<Currency.Api.Grpc.Currency.CurrencyClient>(
            "CurrencyClient", configuration.GetSection("Services:Currency"))
            .AddHttpMessageHandler<TokenPropagationHandler>();

        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IMerchantService, MerchantService>();
        services.AddScoped<IQrService, QrService>();
        services.AddScoped<ICurrencyService, CurrencyService>();

        return services;
    }
}
