namespace Coin.Application.Common;

public record CoinResponse(
    long   CoinId,
    string Name,
    string Code,
    string Symbol,
    bool   IsActive,
    DateTime CreatedAt);
