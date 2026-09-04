namespace Coin.Domain.Errors;

public struct ErrorCode
{
    public const string COIN_NOT_FOUND        = nameof(COIN_NOT_FOUND);
    public const string COIN_INACTIVE         = nameof(COIN_INACTIVE);
    public const string INVALID_EXCHANGE_RATE = nameof(INVALID_EXCHANGE_RATE);
    public const string INSERT_FAILED         = nameof(INSERT_FAILED);
    public const string UPDATE_FAILED         = nameof(UPDATE_FAILED);
    public const string DELETE_FAILED         = nameof(DELETE_FAILED);
}
