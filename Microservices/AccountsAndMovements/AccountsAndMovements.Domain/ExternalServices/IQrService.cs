namespace AccountsAndMovements.Domain.ExternalServices;

/// <summary>Contrato contra el microservicio de QR.</summary>
public interface IQrService
{
    Task<QrInfo?> GetByCode(string code, CancellationToken ct = default);

    /// <summary>
    /// Consume el QR en el servicio de QR (RPC ConsumeQr). Se llama despues de
    /// que el dinero ya se movio: el que impide pagar dos veces el mismo QR es
    /// el indice unico de este servicio, no esta llamada, que puede fallar sin
    /// dejar plata mal contada.
    ///
    /// El contrato oficial no recibe el codigo de transaccion, asi que la traza
    /// cruzada queda del lado de este servicio (MOVI_QR_CODIGO_VC en el asiento
    /// y el log del handler).
    /// </summary>
    Task<bool> Consume(string code, CancellationToken ct = default);
}

/// <summary>
/// Lo que este servicio necesita saber de un QR. Es un subconjunto de QrDataPb:
/// se dejan fuera fecha_creacion, fecha_actualizacion y es_valido porque no
/// cambian ninguna decision de pago -es_valido es solo "estado == ACTIVE", y
/// usarlo perderia el detalle que separa QR_ALREADY_USED de QR_EXPIRED.
///
/// El contrato de QR no publica moneda ni referencia; ver
/// <c>QrService</c> y <c>ExecuteQrPaymentCommandHandler</c> para saber de donde
/// sale cada una.
/// </summary>
/// <param name="QrId">Id del QR en su microservicio; se usa como referencia del asiento.</param>
/// <param name="Amount">Monto que cobra el QR, en la moneda del comercio. 0 = QR abierto.</param>
/// <param name="Type">UNICO | MULTIPLE.</param>
/// <param name="Status">ACTIVE | EXPIRED | USED.</param>
/// <param name="IsActive">Flag "activo": un QR dado de baja llega en false aunque el estado sea ACTIVE.</param>
public record QrInfo(
    long      QrId,
    string    Code,
    long      CommerceId,
    decimal   Amount,
    string    Type,
    string    Status,
    bool      IsActive,
    DateTime? ExpiresAt);
