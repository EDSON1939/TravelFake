using Client.Domain.Entities;
using Client.Domain.Security;
using Client.Domain.Services;
using Client.Infrastructure.Grpc;
using Core.Domain.Errors;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Client.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IAuthService"/> sobre el canal gRPC hacia el
/// microservicio Auth. El canal se configura en
/// <see cref="Extensions.DependencyInjection"/> con la sección
/// Connections:Auth de appsettings.json.
/// </summary>
public class AuthService(Auth.AuthClient authClient, ILogger<AuthService> logger) : IAuthService
{
    /// <summary>Rol con el que Auth da de alta a los clientes del negocio.</summary>
    private const string ClientRole = "CLIENTE";

    public async Task<long?> CreateUser(
        ClientEntity client, ClientCredential credential, CancellationToken ct = default)
    {
        var request = new CreateUserRequestPb
        {
            Username = credential.Username,
            Password = credential.Password,
            // Auth identifica al cliente por "documento"; este microservicio no guarda
            // uno, así que viaja el id con el que acaba de quedar registrado.
            Document = client.CustomerId.ToString(),
            FullName = $"{client.FirstName} {client.LastName}".Trim(),
            Role     = ClientRole
        };

        try
        {
            var response = await authClient.CreateUserAsync(request, cancellationToken: ct);

            // Auth responde con un código de negocio (usuario duplicado, cliente sin
            // credencial disponible...) en lugar de lanzar una excepción.
            if (response.StatusCode != ErrorCode.SUC000)
            {
                logger.LogWarning(
                    "Auth rechazó el usuario {Username} del cliente {CustomerId}: {StatusCode} — {Message}",
                    credential.Username, client.CustomerId, response.StatusCode, response.Message);
                return null;
            }

            return response.Data;
        }
        catch (RpcException ex)
        {
            // El cliente ya está grabado: que Auth esté caído no puede tumbar el registro.
            logger.LogError(ex,
                "No se pudo contactar a Auth para crear el usuario {Username} del cliente {CustomerId}.",
                credential.Username, client.CustomerId);
            return null;
        }
    }
}
