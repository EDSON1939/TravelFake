namespace Coin.Domain.Entities;

public class CoinEntity
{
    public long   CoinId    { get; set; }
    public string Name      { get; set; } = string.Empty;
    public string Code      { get; set; } = string.Empty;
    public string Symbol    { get; set; } = string.Empty;
    public bool   IsActive  { get; set; }
    public DateTime CreatedAt { get; set; }
}
