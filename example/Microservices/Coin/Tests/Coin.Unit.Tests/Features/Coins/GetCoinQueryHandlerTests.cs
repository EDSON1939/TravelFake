using Coin.Application.Features.Coins.Queries.GetCoin;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Coin.Unit.Tests.Features.Coins;

public class GetCoinQueryHandlerTests
{
    private readonly ICoinRepository   _repository = Substitute.For<ICoinRepository>();
    private readonly GetCoinQueryHandler _handler;

    public GetCoinQueryHandlerTests() => _handler = new GetCoinQueryHandler(_repository);

    [Fact]
    public async Task Handle_WhenCoinExists_ReturnsSuccess()
    {
        // Arrange
        var entity = new CoinEntity
        {
            CoinId = 1, Name = "Bitcoin", Code = "BTC",
            Symbol = "₿", IsActive = true, CreatedAt = DateTime.UtcNow
        };
        _repository.GetByCode("BTC", default).Returns(entity);

        // Act
        var result = await _handler.Handle(new GetCoinQuery("BTC"), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("BTC");
        result.Data.Name.Should().Be("Bitcoin");
    }

    [Fact]
    public async Task Handle_WhenCoinNotFound_ReturnsError()
    {
        // Arrange
        _repository.GetByCode("XXX", default).Returns((CoinEntity?)null);

        // Act
        var result = await _handler.Handle(new GetCoinQuery("XXX"), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COIN_NOT_FOUND);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        // Arrange
        _repository.GetByCode(Arg.Any<string>(), default).Returns((CoinEntity?)null);

        // Act
        await _handler.Handle(new GetCoinQuery("BTC"), default);

        // Assert
        await _repository.Received(1).GetByCode("BTC", default);
    }
}
