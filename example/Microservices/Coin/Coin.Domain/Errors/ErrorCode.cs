namespace Coin.Domain.Errors;

public struct ErrorCode
{
    public const string COIN_NOT_FOUND     = nameof(COIN_NOT_FOUND);
    public const string COIN_DUPLICATE     = nameof(COIN_DUPLICATE);
    public const string INSERT_FAILED      = nameof(INSERT_FAILED);
    public const string UPDATE_FAILED      = nameof(UPDATE_FAILED);
}
