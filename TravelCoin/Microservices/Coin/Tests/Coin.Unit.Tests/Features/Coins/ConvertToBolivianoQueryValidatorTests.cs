using Coin.Application.Features.Coins.Queries.ConvertToBoliviano;
using FluentAssertions;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class ConvertToBolivianoQueryValidatorTests
{
    private readonly ConvertToBolivianoQueryValidator _validator = new();

    [Theory]
    [InlineData("100")]
    [InlineData("1500.50")]
    [InlineData("0.01")]
    [InlineData("6.860")]     // extra decimal but no extra precision
    public async Task Validate_WhenAmountIsValid_PassesValidation(string amount)
    {
        var result = await _validator.ValidateAsync(new ConvertToBolivianoQuery(1L, amount));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]          // empty
    [InlineData("abc")]       // not a number
    [InlineData("0")]         // not positive
    [InlineData("-100")]      // negative
    [InlineData("10.555")]    // more than 2 decimals
    [InlineData("1.500,50")]  // comma as decimal separator
    public async Task Validate_WhenAmountIsInvalid_FailsValidation(string amount)
    {
        var result = await _validator.ValidateAsync(new ConvertToBolivianoQuery(1L, amount));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenCoinIdIsNotPositive_FailsValidation()
    {
        var result = await _validator.ValidateAsync(new ConvertToBolivianoQuery(0L, "100"));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenAmountIsEmpty_ReportsASingleError()
    {
        // Cascade(CascadeMode.Stop) keeps an empty value from also firing the format messages
        var result = await _validator.ValidateAsync(new ConvertToBolivianoQuery(1L, ""));
        result.Errors.Should().ContainSingle();
    }
}
