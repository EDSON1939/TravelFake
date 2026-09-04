using AccountsAndMovements.Domain.ExternalServices;
using Core.Domain.Errors;
using Core.Infrastructure.Grpc.Factory;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using System.Globalization;

namespace AccountsAndMovements.Infrastructure.ExternalServices;

/// <summary>
/// Cliente gRPC del microservicio de QR. Traduce QrDataPb -en castellano y con
/// campos que este servicio no usa- al <see cref="QrInfo"/> del dominio.
/// </summary>
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
        var response = await client.GetQrByCodeAsync(
            new Qr.Api.Grpc.GetQrByCodeRequestPb { Code = code }, cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        // Decimal serializado como string: siempre con punto y cultura invariante,
        // nunca con la del proceso, o "139.20" se leeria 13920 en un server es-BO.
        decimal.TryParse(response.Data.Monto, NumberStyles.Number,
                         CultureInfo.InvariantCulture, out var amount);

        // Vacio o no parseable = el QR no expira. TryParse ya devuelve false en
        // ambos casos, asi que no hace falta distinguirlos.
        DateTime? expiresAt = DateTime.TryParse(
            response.Data.FechaExpiracion, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var parsed) ? parsed : null;

        return new QrInfo(
            response.Data.QrId,
            response.Data.Codigo,
            response.Data.ComercioId,
            amount,
            response.Data.Tipo.ToUpperInvariant(),
            response.Data.Estado.ToUpperInvariant(),
            response.Data.Activo,
            expiresAt);
    }

    /// <summary>
    /// Se invoca con el dinero ya movido, asi que un fallo aca NO puede tumbar la
    /// operacion: se devuelve false, el handler lo registra y el pago sigue
    /// siendo valido. Que el QR no se cobre dos veces ya lo garantiza el indice
    /// unico de este servicio, no esta llamada.
    /// </summary>
    public async Task<bool> Consume(string code, CancellationToken ct = default)
    {
        try
        {
            var response = await client.ConsumeQrAsync(
                new Qr.Api.Grpc.ConsumeQrRequestPb { Code = code }, cancellationToken: ct);

            return response.StatusCode == ErrorCode.SUC000;
        }
        catch (RpcException)
        {
            return false;
        }
    }
}
