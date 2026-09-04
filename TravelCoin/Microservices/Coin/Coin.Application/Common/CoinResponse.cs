namespace Coin.Application.Common;

public record CoinResponse(
    long      CoinId,
    string    Name,
    string    Code,
    decimal   BuyRate,
    bool      IsActive,
    DateTime  CreatedAt,
    DateTime? UpdatedAt);
