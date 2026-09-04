namespace Coin.Application.Common;

/// <summary>
/// Result of converting an amount to bolivianos. The applied exchange rate is
/// returned as well so the operation stays auditable on the client side.
/// </summary>
public record ConversionResponse(
    long    CoinId,
    string  Code,
    string  Name,
    decimal ExchangeRate,
    decimal Amount,
    decimal AmountInBs,
    string  TargetCode,
    string  TargetName);
