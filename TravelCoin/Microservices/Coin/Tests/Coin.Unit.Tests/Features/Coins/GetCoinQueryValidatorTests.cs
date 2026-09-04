using Coin.Application.Features.Coins.Queries.GetCoin;
using FluentAssertions;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class GetCoinQueryValidatorTests
{
    private readonly GetCoinQueryValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(9999)]
    public async Task Validate_WhenCoinIdIsPositive_PassesValidation(long coinId)
    {
        var result = await _validator.ValidateAsync(new GetCoinQuery(coinId));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenCoinIdIsNotPositive_FailsValidation(long coinId)
    {
        var result = await _validator.ValidateAsync(new GetCoinQuery(coinId));
        result.IsValid.Should().BeFalse();
    }
}
