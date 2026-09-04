using Auth.Domain.ExternalServices;

namespace Auth.Infrastructure.ExternalServices.Fakes;

/// <summary>
/// Datos quemados del microservicio de Clientes, que todavia no existe. Se
/// registra solo cuando "UseFakeExternalServices" esta en true; el cableado gRPC
/// real queda intacto y vuelve a usarse apagando esa bandera.
///
/// Los IDs coinciden con los del fake de AccountsAndMovements: asi el cliente 1
/// que se autentica aca es el mismo que tiene cuenta en USD alla.
/// </summary>
public class FakeClientService : IClientService
{
    private static readonly Dictionary<string, ClientInfo> Clients =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["12345678"] = new ClientInfo(1, "12345678", "Carlos Perez", IsActive: true),
            ["87654321"] = new ClientInfo(2, "87654321", "Ana Da Silva", IsActive: true),
            ["11223344"] = new ClientInfo(3, "11223344", "Luis Mamani",  IsActive: true),
            // Para probar CLIENT_INACTIVE.
            ["99999999"] = new ClientInfo(9, "99999999", "Cliente Baja", IsActive: false),
        };

    public Task<ClientInfo?> GetByDocument(string document, CancellationToken ct = default)
        => Task.FromResult<ClientInfo?>(Clients.TryGetValue(document, out var client) ? client : null);
}
