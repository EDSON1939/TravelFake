using Coin.Application.Features.Coins.Queries.GetCoin;
using FluentAssertions;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class GetCoinQueryValidatorTests
{
    private readonly GetCoinQueryValidator _validator = new();

    [Theory]
    [InlineData("BTC")]
    [InlineData("ETH")]
    [InlineData("USDT10")]
    public async Task Validate_WhenCodeIsValid_PassesValidation(string code)
    {
        var result = await _validator.ValidateAsync(new GetCoinQuery(code));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("TOOLONGCODE123")]
    [InlineData("btc")]
    [InlineData("bt c")]
    public async Task Validate_WhenCodeIsInvalid_FailsValidation(string code)
    {
        var result = await _validator.ValidateAsync(new GetCoinQuery(code));
        result.IsValid.Should().BeFalse();
    }
}
