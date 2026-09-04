using Coin.Application.Features.Coins.Queries.ConvertToBoliviano;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class ConvertToBolivianoQueryHandlerTests
{
    private readonly ICoinRepository                _repository = Substitute.For<ICoinRepository>();
    private readonly ConvertToBolivianoQueryHandler _handler;

    public ConvertToBolivianoQueryHandlerTests() => _handler = new ConvertToBolivianoQueryHandler(_repository);

    private static CoinEntity Dollar(decimal buyRate = 6.86m, bool isActive = true) => new()
    {
        CoinId = 1, Name = "Dólar estadounidense", Code = "USD",
        BuyRate = buyRate, IsActive = isActive, CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_ConvertsAmountUsingBuyRate()
    {
        // Arrange
        _repository.GetById(1L, default).Returns(Dollar());

        // Act — 100 USD * 6.86 = 686.00 Bs
        var result = await _handler.Handle(new ConvertToBolivianoQuery(1L, "100"), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().NotBeNull();
        result.Data!.AmountInBs.Should().Be(686.00m);
        result.Data.Amount.Should().Be(100m);
        result.Data.ExchangeRate.Should().Be(6.86m);
        result.Data.TargetCode.Should().Be("BOB");
    }

    [Fact]
    public async Task Handle_RoundsToTwoDecimals()
    {
        // Arrange
        _repository.GetById(1L, default).Returns(Dollar());

        // Act — 10.55 * 6.86 = 72.373 → 72.37
        var result = await _handler.Handle(new ConvertToBolivianoQuery(1L, "10.55"), default);

        // Assert
        result.Data!.AmountInBs.Should().Be(72.37m);
    }

    [Fact]
    public async Task Handle_RoundsHalfAwayFromZero()
    {
        // Arrange — a rate that produces exactly .005
        _repository.GetById(1L, default).Returns(Dollar(buyRate: 0.05m));

        // Act — 12.10 * 0.05 = 0.605 → 0.61 (not 0.60 as banker's rounding would give)
        var result = await _handler.Handle(new ConvertToBolivianoQuery(1L, "12.10"), default);

        // Assert
        result.Data!.AmountInBs.Should().Be(0.61m);
    }

    [Fact]
    public async Task Handle_WhenCoinIsBoliviano_ReturnsSameAmount()
    {
        // Arrange — the boliviano is registered with an exchange rate of 1
        _repository.GetById(9L, default).Returns(new CoinEntity
        {
            CoinId = 9, Name = "Boliviano", Code = "BOB", BuyRate = 1m, IsActive = true
        });

        // Act
        var result = await _handler.Handle(new ConvertToBolivianoQuery(9L, "250.75"), default);

        // Assert
        result.Data!.AmountInBs.Should().Be(250.75m);
    }

    [Fact]
    public async Task Handle_WhenCoinNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _repository.GetById(99L, default).Returns((CoinEntity?)null);

        // Act
        var result = await _handler.Handle(new ConvertToBolivianoQuery(99L, "100"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_NOT_FOUND);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCoinIsInactive_ReturnsInactiveError()
    {
        // Arrange
        _repository.GetById(1L, default).Returns(Dollar(isActive: false));

        // Act
        var result = await _handler.Handle(new ConvertToBolivianoQuery(1L, "100"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_INACTIVE);
    }

    [Fact]
    public async Task Handle_WhenBuyRateIsZero_ReturnsInvalidRateError()
    {
        // Arrange
        _repository.GetById(1L, default).Returns(Dollar(buyRate: 0m));

        // Act
        var result = await _handler.Handle(new ConvertToBolivianoQuery(1L, "100"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INVALID_EXCHANGE_RATE);
    }
}
