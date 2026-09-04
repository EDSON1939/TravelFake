using AccountsAndMovements.Domain.ExternalServices;

namespace AccountsAndMovements.Infrastructure.ExternalServices.Fakes;

/// <summary>Datos quemados del microservicio de Comercios. Todos cobran en BOB.</summary>
public class FakeMerchantService : IMerchantService
{
    private static readonly Dictionary<long, MerchantInfo> Merchants = new()
    {
        [55] = new MerchantInfo(55, "Cafe Central",   "1023456789", "BOB", IsActive: true),
        [56] = new MerchantInfo(56, "Artesanias Sol", "1098765432", "BOB", IsActive: true),
        // Para probar MERCHANT_INACTIVE.
        [99] = new MerchantInfo(99, "Comercio Baja",  "1000000000", "BOB", IsActive: false),
    };

    public Task<MerchantInfo?> GetById(long merchantId, CancellationToken ct = default)
        => Task.FromResult<MerchantInfo?>(Merchants.TryGetValue(merchantId, out var merchant) ? merchant : null);
}
