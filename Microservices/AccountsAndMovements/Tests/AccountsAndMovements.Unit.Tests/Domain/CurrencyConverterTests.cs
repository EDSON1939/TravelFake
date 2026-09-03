using AccountsAndMovements.Domain.Services;
using FluentAssertions;
using Xunit;

namespace AccountsAndMovements.Unit.Tests.Domain;

public class CurrencyConverterTests
{
    [Fact]
    public void Convert_UsesTheRateAndRoundsToTwoDecimals()
    {
        // El ejemplo del reto: 20 USD x 6.96 = 139.20 BOB
        CurrencyConverter.Convert(20m, 6.96m).Should().Be(139.20m);
    }

    [Theory]
    [InlineData(100, 1.85, 185.00)]
    [InlineData(2500, 1.92, 4800.00)]
    [InlineData(15.5, 6.96, 107.88)]
    public void Convert_ConvertsTheDocumentedPairs(decimal amount, decimal rate, decimal expected)
    {
        CurrencyConverter.Convert(amount, rate).Should().Be(expected);
    }

    [Fact]
    public void Convert_RoundsHalfAwayFromZero()
    {
        // 1.005 redondea a 1.01, no a 1.00: es el criterio contable, y con
        // MidpointRounding.ToEven el comercio cobraria un centavo de menos.
        CurrencyConverter.Convert(0.1005m, 10m).Should().Be(1.01m);
    }

    [Fact]
    public void AmountsMatch_ComparesAtMoneyScale()
    {
        CurrencyConverter.AmountsMatch(139.20m, 139.2000m).Should().BeTrue();
        CurrencyConverter.AmountsMatch(139.20m, 139.21m).Should().BeFalse();
    }
}
