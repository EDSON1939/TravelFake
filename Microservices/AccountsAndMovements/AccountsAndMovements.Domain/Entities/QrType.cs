namespace AccountsAndMovements.Domain.Entities;

/// <summary>
/// Tipos de QR del contrato oficial (campo "tipo").
///
/// Este servicio solo puede liquidar UNICO. MULTIPLE significa que el mismo
/// codigo se cobra varias veces, y el libro mayor lo prohibe de raiz: el indice
/// UQ_COMMERCE_MOVIMIENTO_QR admite un unico DEBITO por codigo de QR. Aceptarlo
/// daria un primer pago bueno y un QR_ALREADY_USED enganoso en el segundo, asi
/// que el handler lo rechaza de entrada con QR_TYPE_NOT_SUPPORTED.
/// </summary>
public struct QrType
{
    public const string UNICO    = nameof(UNICO);
    public const string MULTIPLE = nameof(MULTIPLE);
}
