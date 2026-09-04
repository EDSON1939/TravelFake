using Coin.Application.Features.Coins.Queries.GetCoin;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class GetCoinQueryHandlerTests
{
    private readonly ICoinRepository     _repository = Substitute.For<ICoinRepository>();
    private readonly GetCoinQueryHandler _handler;

    public GetCoinQueryHandlerTests() => _handler = new GetCoinQueryHandler(_repository);

    [Fact]
    public async Task Handle_WhenCoinExists_ReturnsSuccess()
    {
        // Arrange
        _repository.GetById(1L, default).Returns(new CoinEntity
        {
            CoinId = 1, Name = "Dólar estadounidense", Code = "USD",
            BuyRate = 6.86m, IsActive = true, CreatedAt = DateTime.UtcNow
        });

        // Act
        var result = await _handler.Handle(new GetCoinQuery(1L), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.Code.Should().Be("USD");
        result.Data.BuyRate.Should().Be(6.86m);
    }

    [Fact]
    public async Task Handle_WhenCoinNotFound_ReturnsError()
    {
        // Arrange
        _repository.GetById(99L, default).Returns((CoinEntity?)null);

        // Act
        var result = await _handler.Handle(new GetCoinQuery(99L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_NOT_FOUND);
        result.Data.Should().BeNull();
    }
}
