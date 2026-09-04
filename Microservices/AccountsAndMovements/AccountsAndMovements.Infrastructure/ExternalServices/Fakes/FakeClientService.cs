using AccountsAndMovements.Domain.ExternalServices;

namespace AccountsAndMovements.Infrastructure.ExternalServices.Fakes;

/// <summary>
/// Datos quemados del microservicio de Clientes, que todavia no existe. Se
/// registra solo cuando "UseFakeExternalServices" esta en true; el cableado gRPC
/// real queda intacto y vuelve a usarse apagando esa bandera.
/// </summary>
public class FakeClientService : IClientService
{
    private static readonly Dictionary<long, ClientInfo> Clients = new()
    {
        [1] = new ClientInfo(1, "Carlos Perez",  "carlos@mail.com", "PE", "USD", IsActive: true),
        [2] = new ClientInfo(2, "Ana Da Silva",  "ana@mail.com",    "BR", "EUR", IsActive: true),
        [3] = new ClientInfo(3, "Luis Mamani",   "luis@mail.com",   "BO", "BOB", IsActive: true),
        // Para probar CUSTOMER_INACTIVE.
        [9] = new ClientInfo(9, "Cliente Baja",  "baja@mail.com",   "PE", "USD", IsActive: false),
    };

    public Task<ClientInfo?> GetById(long clientId, CancellationToken ct = default)
        => Task.FromResult<ClientInfo?>(Clients.TryGetValue(clientId, out var client) ? client : null);
}
