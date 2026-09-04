using Commerce.Application.Features.Commerces.Commands.CreateCommerce;
using Commerce.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Commerce.Unit.Tests.Features.Commerces;

public class CreateCommerceCommandHandlerTests
{
    private readonly ICommerceRepository          _repository = Substitute.For<ICommerceRepository>();
    private readonly CreateCommerceCommandHandler _handler;

    public CreateCommerceCommandHandlerTests() => _handler = new CreateCommerceCommandHandler(_repository);

    [Fact]
    public async Task Handle_WhenCommerceIsNew_ReturnsSuccessWithId()
    {
        _repository.ExistsByNit("900123456", default).Returns(false);
        _repository.Insert(Arg.Any<Domain.Entities.CommerceEntity>(), default).Returns(7L);

        var result = await _handler.Handle(
            new CreateCommerceCommand("Tienda Central", "900123456"), default);

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(7L);
    }

    [Fact]
    public async Task Handle_WhenNitAlreadyExists_ReturnsDuplicateError()
    {
        _repository.ExistsByNit("900123456", default).Returns(true);

        var result = await _handler.Handle(
            new CreateCommerceCommand("Tienda Central", "900123456"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COMMERCE_DUPLICATE);
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertError()
    {
        _repository.ExistsByNit("900123457", default).Returns(false);
        _repository.Insert(Arg.Any<Domain.Entities.CommerceEntity>(), default).Returns(0L);

        var result = await _handler.Handle(
            new CreateCommerceCommand("Tienda Norte", "900123457"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
    }

    [Fact]
    public async Task Handle_StartsWithActiveStateAndNullCuentaId()
    {
        _repository.ExistsByNit(Arg.Any<string>(), default).Returns(false);
        _repository.Insert(Arg.Any<Domain.Entities.CommerceEntity>(), default).Returns(1L);
        Domain.Entities.CommerceEntity? captured = null;
        await _repository.Insert(Arg.Do<Domain.Entities.CommerceEntity>(e => captured = e), default);

        await _handler.Handle(new CreateCommerceCommand("Tienda", "900123458"), default);

        captured?.IsActive.Should().BeTrue();
        captured?.CuentaId.Should().BeNull();
    }
}
