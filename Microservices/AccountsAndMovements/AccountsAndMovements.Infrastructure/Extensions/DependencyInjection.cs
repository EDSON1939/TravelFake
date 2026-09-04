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

        // Reenvia el JWT recibido hacia el siguiente salto de la cadena. Se
        // registra siempre: lo necesita cualquiera de los clientes gRPC que se
        // active abajo, y si no se activa ninguno no cuesta nada.
        services.AddTransient<TokenPropagationHandler>();

        // Los cuatro servicios externos se deciden uno por uno: el que ya esta
        // desplegado va por gRPC y el que no, contra un catalogo en memoria.
        // Ver UsarFake.
        AddClientService(services, configuration);
        AddCommerceService(services, configuration);
        AddQrService(services, configuration);
        AddCurrencyService(services, configuration);

        return services;
    }

    /// <summary>
    /// Decide si un servicio externo se resuelve contra el microservicio real o
    /// contra el catalogo en memoria.
    ///
    /// "UseFakeExternalServices" fija el valor por defecto de los cuatro y
    /// "Services:&lt;Nombre&gt;:UseFake" lo pisa para uno solo. Esa granularidad es
    /// justamente lo que permite apuntar Comercios al microservicio real sin
    /// arrastrar a Clientes, QR y Monedas, que todavia no estan desplegados: con
    /// una sola bandera global habria que levantar los cuatro o ninguno.
    /// </summary>
    private static bool UsarFake(IConfiguration configuration, string service)
        => configuration.GetValue<bool?>($"Services:{service}:UseFake")
           ?? configuration.GetValue<bool>("UseFakeExternalServices");

    private static void AddClientService(IServiceCollection services, IConfiguration configuration)
    {
        if (UsarFake(configuration, "Client"))
        {
            services.AddSingleton<IClientService, FakeClientService>();
            return;
        }

        services.AddGrpcClientDependence<Client.Api.Grpc.Client.ClientClient>(
            "ClientClient", configuration.GetSection("Services:Client"))
            .AddHttpMessageHandler<TokenPropagationHandler>();

        services.AddScoped<IClientService, ClientService>();
    }

    /// <summary>
    /// El retry y el circuit breaker se enganchan solos si la seccion
    /// "Services:Commerce" declara PolicyConfiguration y CircuitBreaker; sin
    /// esas secciones el cliente queda sin politicas, que es lo correcto para
    /// un destino local.
    /// </summary>
    private static void AddCommerceService(IServiceCollection services, IConfiguration configuration)
    {
        if (UsarFake(configuration, "Commerce"))
        {
            services.AddSingleton<ICommerceService, FakeCommerceService>();
            return;
        }

        services.AddGrpcClientDependence<Commerce.Api.Grpc.Commerce.CommerceClient>(
            "CommerceClient", configuration.GetSection("Services:Commerce"))
            .AddHttpMessageHandler<TokenPropagationHandler>();

        services.AddScoped<ICommerceService, CommerceService>();
    }

    /// <summary>
    /// FakeQrService va como Singleton porque Consume cambia el estado del QR
    /// y ese cambio tiene que sobrevivir entre requests; los otros tres fakes son
    /// catalogos inmutables y el lifetime les da igual.
    /// </summary>
    private static void AddQrService(IServiceCollection services, IConfiguration configuration)
    {
        if (UsarFake(configuration, "Qr"))
        {
            services.AddSingleton<IQrService, FakeQrService>();
            return;
        }

        services.AddGrpcClientDependence<Qr.Api.Grpc.Qr.QrClient>(
            "QrClient", configuration.GetSection("Services:Qr"))
            .AddHttpMessageHandler<TokenPropagationHandler>();

        services.AddScoped<IQrService, QrService>();
    }

    private static void AddCurrencyService(IServiceCollection services, IConfiguration configuration)
    {
        if (UsarFake(configuration, "Currency"))
        {
            services.AddSingleton<ICurrencyService, FakeCurrencyService>();
            return;
        }

        services.AddGrpcClientDependence<Currency.Api.Grpc.Currency.CurrencyClient>(
            "CurrencyClient", configuration.GetSection("Services:Currency"))
            .AddHttpMessageHandler<TokenPropagationHandler>();

        services.AddScoped<ICurrencyService, CurrencyService>();
    }
}
