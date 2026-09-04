namespace AccountsAndMovements.Application.Common;

/// <summary>
/// Asiento del libro mayor tal como se devuelve al llamador. Incluye los datos
/// de conversion congelados: el historial se lee sin volver a consultar el tipo
/// de cambio, que para entonces ya pudo cambiar.
/// </summary>
public record MovementResponse(
    long     MovementId,
    string   AccountNumber,
    string   AccountType,
    long     HolderId,
    string   Type,
    string   Status,
    decimal  Amount,
    decimal  BalanceBefore,
    decimal  BalanceAfter,
    decimal  OriginalAmount,
    string   OriginalCurrency,
    decimal  ExchangeRate,
    decimal  ConvertedAmount,
    string   TargetCurrency,
    long?    CommerceId,
    string?  QrCode,
    string   Reference,
    string   IdempotencyKey,
    string   TransactionCode,
    string   Description,
    DateTime CreatedAt);
