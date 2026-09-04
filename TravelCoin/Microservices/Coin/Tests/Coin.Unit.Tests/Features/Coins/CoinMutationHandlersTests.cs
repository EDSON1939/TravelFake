using Coin.Application.Features.Coins.Commands.DeleteCoin;
using Coin.Application.Features.Coins.Commands.UpdateCoin;
using Coin.Application.Features.Coins.Commands.UpdateCoinStatus;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class CoinMutationHandlersTests
{
    private readonly ICoinRepository _repository = Substitute.For<ICoinRepository>();

    [Fact]
    public async Task Update_WhenRowAffected_ReturnsSuccess()
    {
        // Arrange
        _repository.Update(Arg.Any<CoinEntity>(), default).Returns(1L);
        var handler = new UpdateCoinCommandHandler(_repository);

        // Act
        var result = await handler.Handle(
            new UpdateCoinCommand(1L, "Euro", "EUR", "7.45", true), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(1L);
    }

    [Fact]
    public async Task Update_WhenCoinNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _repository.Update(Arg.Any<CoinEntity>(), default).Returns(0L);
        var handler = new UpdateCoinCommandHandler(_repository);

        // Act
        var result = await handler.Handle(
            new UpdateCoinCommand(99L, "Euro", "EUR", "7.45", true), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_NOT_FOUND);
    }

    [Fact]
    public async Task UpdateStatus_WhenCoinNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _repository.UpdateStatus(99L, false, default).Returns(0L);
        var handler = new UpdateCoinStatusCommandHandler(_repository);

        // Act
        var result = await handler.Handle(new UpdateCoinStatusCommand(99L, false), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_NOT_FOUND);
    }

    [Fact]
    public async Task Delete_WhenRowAffected_ReturnsSuccess()
    {
        // Arrange
        _repository.Delete(5L, default).Returns(1L);
        var handler = new DeleteCoinCommandHandler(_repository);

        // Act
        var result = await handler.Handle(new DeleteCoinCommand(5L), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(5L);
    }

    [Fact]
    public async Task Delete_WhenCoinNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _repository.Delete(99L, default).Returns(0L);
        var handler = new DeleteCoinCommandHandler(_repository);

        // Act
        var result = await handler.Handle(new DeleteCoinCommand(99L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_NOT_FOUND);
    }
}
