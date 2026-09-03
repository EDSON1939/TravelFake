namespace AccountsAndMovements.Application.Common;

public record AccountResponse(
    long      AccountId,
    string    Number,
    string    OwnerType,
    long      OwnerId,
    long      CoinId,
    string    CoinCode,
    decimal   Balance,
    bool      IsActive,
    DateTime  CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt);
