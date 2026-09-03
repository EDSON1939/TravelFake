namespace AccountsAndMovements.Infrastructure.Persistence.Queries;

/// <summary>
/// Proyeccion comun del libro mayor. Vive en un solo lugar porque las cuatro
/// consultas de movimientos devuelven exactamente la misma forma y solo cambian
/// el filtro: duplicarla seria garantizar que alguna quede desincronizada.
/// </summary>
internal static class MovementProjection
{
    public const string SelectFrom = @"
        SELECT m.MOVI_ID_IT               AS MovementId,
               m.MOVI_CUENTA_ID_IT        AS AccountId,
               c.CUEN_NUMERO_VC           AS AccountNumber,
               c.CUEN_TITULAR_TIPO_VC     AS OwnerType,
               c.CUEN_TITULAR_ID_IT       AS OwnerId,
               m.MOVI_TIPO_VC             AS Type,
               m.MOVI_ESTADO_VC           AS Status,
               m.MOVI_MONTO_DE            AS Amount,
               m.MOVI_SALDO_ANTERIOR_DE   AS BalanceBefore,
               m.MOVI_SALDO_POSTERIOR_DE  AS BalanceAfter,
               m.MOVI_MONTO_ORIGEN_DE     AS OriginalAmount,
               m.MOVI_MONEDA_ORIGEN_VC    AS OriginalCurrency,
               m.MOVI_TIPO_CAMBIO_DE      AS ExchangeRate,
               m.MOVI_MONTO_DESTINO_DE    AS ConvertedAmount,
               m.MOVI_MONEDA_DESTINO_VC   AS TargetCurrency,
               m.MOVI_COMERCIO_ID_IT      AS MerchantId,
               m.MOVI_QR_CODIGO_VC        AS QrCode,
               m.MOVI_REFERENCIA_VC       AS Reference,
               m.MOVI_IDEMPOTENCIA_VC     AS IdempotencyKey,
               m.MOVI_TRANSACCION_VC      AS TransactionCode,
               m.MOVI_DESCRIPCION_VC      AS Description,
               m.MOVI_FECHA_CREACION_DT   AS CreatedAt
        FROM   pay.MOVIMIENTO m
        INNER JOIN pay.CUENTA c ON c.CUEN_ID_IT = m.MOVI_CUENTA_ID_IT";
}
