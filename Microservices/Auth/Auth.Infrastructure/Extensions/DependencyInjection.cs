using Auth.Domain.ExternalServices;
using Auth.Domain.Repositories;
using Auth.Domain.Security;
using Auth.Infrastructure.ExternalServices;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Security;
using Core.Infrastructure.Extensions;
using Core.Infrastructure.Grpc;
using Core.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseDependence(configuration);
        services.AddScoped<IUserRepository, UserRepository>();

        // Reenvía el JWT recibido hacia el siguiente salto de la cadena.
        services.AddTransient<TokenPropagationHandler>();

        services.AddGrpcClientDependence<Client.Api.Grpc.Client.ClientClient>(
            "ClientClient", configuration.GetSection("Services:Client"))
            .AddHttpMessageHandler<TokenPropagationHandler>();
        services.AddScoped<IClientService, ClientService>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
