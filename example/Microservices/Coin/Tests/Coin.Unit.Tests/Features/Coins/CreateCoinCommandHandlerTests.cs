using Coin.Application.Features.Coins.Commands.CreateCoin;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class CreateCoinCommandHandlerTests
{
    private readonly ICoinRepository         _repository = Substitute.For<ICoinRepository>();
    private readonly CreateCoinCommandHandler _handler;

    public CreateCoinCommandHandlerTests() => _handler = new CreateCoinCommandHandler(_repository);

    [Fact]
    public async Task Handle_WhenCoinIsNew_ReturnsSuccessWithId()
    {
        // Arrange
        _repository.GetByCode("ETH", default).Returns((CoinEntity?)null);
        _repository.Insert(Arg.Any<CoinEntity>(), default).Returns(42L);

        // Act
        var result = await _handler.Handle(
            new CreateCoinCommand("Ethereum", "ETH", "Ξ"), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(42L);
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_ReturnsDuplicateError()
    {
        // Arrange
        _repository.GetByCode("BTC", default).Returns(new CoinEntity { Code = "BTC" });

        // Act
        var result = await _handler.Handle(
            new CreateCoinCommand("Bitcoin", "BTC", "₿"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_DUPLICATE);
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertError()
    {
        // Arrange
        _repository.GetByCode("SOL", default).Returns((CoinEntity?)null);
        _repository.Insert(Arg.Any<CoinEntity>(), default).Returns(0L);

        // Act
        var result = await _handler.Handle(
            new CreateCoinCommand("Solana", "SOL", "◎"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
    }

    [Fact]
    public async Task Handle_NormalizesCodeToUpperCase()
    {
        // Arrange
        _repository.GetByCode("ETH", default).Returns((CoinEntity?)null);
        _repository.Insert(Arg.Any<CoinEntity>(), default).Returns(1L);
        CoinEntity? captured = null;
        await _repository.Insert(Arg.Do<CoinEntity>(e => captured = e), default);

        // Act
        await _handler.Handle(new CreateCoinCommand("Ethereum", "eth", "Ξ"), default);

        // Assert
        captured?.Code.Should().Be("ETH");
    }
}
