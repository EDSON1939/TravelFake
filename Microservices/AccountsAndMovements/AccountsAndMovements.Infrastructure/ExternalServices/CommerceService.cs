using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using Core.Domain.Errors;
using Core.Infrastructure.Grpc.Factory;
using Grpc.Net.ClientFactory;

namespace AccountsAndMovements.Infrastructure.ExternalServices;

/// <summary>Cliente gRPC del microservicio de Comercios.</summary>
public class CommerceService(GrpcClientFactory grpcClientFactory)
    : GrpcClientFactory<Commerce.Api.Grpc.Commerce.CommerceClient>(grpcClientFactory), ICommerceService
{
    public override string Name => "CommerceClient";

    public async Task<CommerceInfo?> GetById(long commerceId, CancellationToken ct = default)
    {
        var response = await client.GetCommerceByIdAsync(
            new Commerce.Api.Grpc.GetCommerceByIdRequestPb { Id = commerceId },
            cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        return new CommerceInfo(
            response.Data.Id,
            response.Data.Name,
            response.Data.Nit,
            // El contrato de Comercios no publica moneda: por regla del reto el
            // comercio boliviano siempre cobra en BOB.
            CurrencyCode.BOB,
            response.Data.IsActive);
    }
}
