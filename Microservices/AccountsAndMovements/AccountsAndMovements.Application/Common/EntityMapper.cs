using AccountsAndMovements.Domain.Entities;

namespace AccountsAndMovements.Application.Common;

/// <summary>
/// Traduccion entidad -> respuesta. Esta en un solo lugar porque cinco handlers
/// devuelven cuentas o asientos: repetir el mapeo era la forma segura de que
/// alguno quedara con un campo de menos.
/// </summary>
internal static class EntityMapper
{
    public static AccountResponse ToResponse(this AccountEntity e)
        => new(e.AccountId, e.Number, e.OwnerType, e.OwnerId, e.CoinId, e.CoinCode,
               e.Balance, e.IsActive, e.CreatedAt, e.UpdatedAt, e.DeletedAt);

    public static MovementResponse ToResponse(this MovementEntity e)
        => new(e.MovementId, e.AccountNumber, e.OwnerType, e.OwnerId, e.Type, e.Status,
               e.Amount, e.BalanceBefore, e.BalanceAfter,
               e.OriginalAmount, e.OriginalCurrency, e.ExchangeRate,
               e.ConvertedAmount, e.TargetCurrency,
               e.MerchantId, e.QrCode, e.Reference, e.IdempotencyKey,
               e.TransactionCode, e.Description, e.CreatedAt);

    public static List<MovementResponse> ToResponse(this IEnumerable<MovementEntity> entities)
        => entities.Select(ToResponse).ToList();
}
