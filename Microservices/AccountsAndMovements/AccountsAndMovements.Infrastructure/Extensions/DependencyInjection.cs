using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using AccountsAndMovements.Infrastructure.ExternalServices;
using AccountsAndMovements.Infrastructure.ExternalServices.Fakes;
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

        // Clientes, Comercios, QR y Monedas todavia no existen como servicios
        // desplegados. Con "UseFakeExternalServices": true se reemplazan por
        // catalogos en memoria y este microservicio corre solo. Apagar la
        // bandera devuelve el cableado gRPC real sin tocar codigo.
        if (configuration.GetValue<bool>("UseFakeExternalServices"))
            return services.AddFakeExternalServices();

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

    /// <summary>
    /// Datos quemados para probar el servicio aislado. FakeQrService va como
    /// Singleton porque MarkAsUsed cambia el estado del QR y ese cambio tiene que
    /// sobrevivir entre requests; los otros tres son catalogos inmutables.
    /// </summary>
    private static IServiceCollection AddFakeExternalServices(this IServiceCollection services)
    {
        services.AddSingleton<IClientService,   FakeClientService>();
        services.AddSingleton<IMerchantService, FakeMerchantService>();
        services.AddSingleton<IQrService,       FakeQrService>();
        services.AddSingleton<ICurrencyService, FakeCurrencyService>();

        return services;
    }
}
