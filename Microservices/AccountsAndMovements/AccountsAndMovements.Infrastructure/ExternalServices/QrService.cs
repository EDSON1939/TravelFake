using AccountsAndMovements.Domain.ExternalServices;
using Core.Domain.Errors;
using Core.Infrastructure.Grpc.Factory;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using System.Globalization;

namespace AccountsAndMovements.Infrastructure.ExternalServices;

/// <summary>Cliente gRPC del microservicio de QR.</summary>
public class QrService(GrpcClientFactory grpcClientFactory)
    : GrpcClientFactory<Qr.Api.Grpc.Qr.QrClient>(grpcClientFactory), IQrService
{
    public override string Name => "QrClient";

    /// <summary>
    /// Lectura previa al pago. Si QR no responde, la operacion no puede seguir y
    /// la excepcion se propaga: pagar sin haber leido el QR seria peor.
    /// </summary>
    public async Task<QrInfo?> GetByCode(string code, CancellationToken ct = default)
    {
        var response = await client.GetQrAsync(
            new Qr.Api.Grpc.GetQrRequestPb { Code = code }, cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        decimal.TryParse(response.Data.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount);

        DateTime? expiresAt = DateTime.TryParse(
            response.Data.ExpiresAt, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var parsed) ? parsed : null;

        return new QrInfo(
            response.Data.Code,
            response.Data.MerchantId,
            amount,
            response.Data.CurrencyCode.ToUpperInvariant(),
            response.Data.Reference,
            response.Data.Status.ToUpperInvariant(),
            expiresAt);
    }

    /// <summary>
    /// Se invoca con el dinero ya movido, asi que un fallo aca NO puede tumbar la
    /// operacion: se devuelve false, el handler lo registra y el pago sigue
    /// siendo valido. Que el QR no se cobre dos veces ya lo garantiza el indice
    /// unico de este servicio, no esta llamada.
    /// </summary>
    public async Task<bool> MarkAsUsed(string code, string transactionCode, CancellationToken ct = default)
    {
        try
        {
            var response = await client.MarkQrAsUsedAsync(
                new Qr.Api.Grpc.MarkQrAsUsedRequestPb
                {
                    Code            = code,
                    TransactionCode = transactionCode
                }, cancellationToken: ct);

            return response.StatusCode == ErrorCode.SUC000;
        }
        catch (RpcException)
        {
            return false;
        }
    }
}
