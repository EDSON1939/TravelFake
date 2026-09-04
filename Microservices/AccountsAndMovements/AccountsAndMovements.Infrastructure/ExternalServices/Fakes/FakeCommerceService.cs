using AccountsAndMovements.Domain.ExternalServices;

namespace AccountsAndMovements.Infrastructure.ExternalServices.Fakes;

/// <summary>Datos quemados del microservicio de Comercios. Todos cobran en BOB.</summary>
public class FakeCommerceService : ICommerceService
{
    private static readonly Dictionary<long, CommerceInfo> Commerces = new()
    {
        [55] = new CommerceInfo(55, "Cafe Central",   "1023456789", "BOB", IsActive: true),
        [56] = new CommerceInfo(56, "Artesanias Sol", "1098765432", "BOB", IsActive: true),
        // Para probar COMMERCE_INACTIVE.
        [99] = new CommerceInfo(99, "Comercio Baja",  "1000000000", "BOB", IsActive: false),
    };

    public Task<CommerceInfo?> GetById(long commerceId, CancellationToken ct = default)
        => Task.FromResult<CommerceInfo?>(Commerces.TryGetValue(commerceId, out var commerce) ? commerce : null);
}
