using AccountsAndMovements.Domain.ExternalServices;
using Core.Domain.Errors;
using Core.Infrastructure.Grpc.Factory;
using Grpc.Net.ClientFactory;

namespace AccountsAndMovements.Infrastructure.ExternalServices;

/// <summary>Cliente gRPC del microservicio de Comercios.</summary>
public class MerchantService(GrpcClientFactory grpcClientFactory)
    : GrpcClientFactory<Merchant.Api.Grpc.Merchant.MerchantClient>(grpcClientFactory), IMerchantService
{
    private const string ActiveStatus = "ACTIVE";

    public override string Name => "MerchantClient";

    public async Task<MerchantInfo?> GetById(long merchantId, CancellationToken ct = default)
    {
        var response = await client.GetMerchantByIdAsync(
            new Merchant.Api.Grpc.GetMerchantByIdRequestPb { MerchantId = merchantId },
            cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        return new MerchantInfo(
            response.Data.MerchantId,
            response.Data.Name,
            response.Data.Nit,
            response.Data.CurrencyCode.ToUpperInvariant(),
            response.Data.Status.Equals(ActiveStatus, StringComparison.OrdinalIgnoreCase));
    }
}
