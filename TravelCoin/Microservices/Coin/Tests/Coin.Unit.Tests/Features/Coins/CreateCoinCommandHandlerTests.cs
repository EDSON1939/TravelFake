using Coin.Application.Features.Coins.Commands.CreateCoin;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class CreateCoinCommandHandlerTests
{
    private readonly ICoinRepository          _repository = Substitute.For<ICoinRepository>();
    private readonly CreateCoinCommandHandler _handler;

    public CreateCoinCommandHandlerTests() => _handler = new CreateCoinCommandHandler(_repository);

    [Fact]
    public async Task Handle_WhenInsertSucceeds_ReturnsSuccessWithId()
    {
        // Arrange
        _repository.Insert(Arg.Any<CoinEntity>(), default).Returns(42L);

        // Act
        var result = await _handler.Handle(
            new CreateCoinCommand("Dólar estadounidense", "USD", "6.86"), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(42L);
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertError()
    {
        // Arrange
        _repository.Insert(Arg.Any<CoinEntity>(), default).Returns(0L);

        // Act
        var result = await _handler.Handle(new CreateCoinCommand("Euro", "EUR", "7.45"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
    }

    [Fact]
    public async Task Handle_ParsesBuyRateWithInvariantCultureAndNormalizesCode()
    {
        // Arrange
        CoinEntity? captured = null;
        _repository.Insert(Arg.Do<CoinEntity>(e => captured = e), default).Returns(1L);

        // Act
        await _handler.Handle(new CreateCoinCommand("  Euro  ", "eur", "7.45"), default);

        // Assert
        captured.Should().NotBeNull();
        captured!.BuyRate.Should().Be(7.45m);
        captured.Code.Should().Be("EUR");
        captured.Name.Should().Be("Euro");
    }
}
