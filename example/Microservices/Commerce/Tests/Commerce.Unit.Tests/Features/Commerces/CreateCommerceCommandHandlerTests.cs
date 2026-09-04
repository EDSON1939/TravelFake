using Commerce.Application.Features.Commerces.Commands.CreateCommerce;
using Commerce.Domain.Interfaces;
using Commerce.Domain.Repositories;
using Core.Domain.Errors;
using Core.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Commerce.Unit.Tests.Features.Commerces;

public class CreateCommerceCommandHandlerTests
{
    private readonly ICommerceRepository                 _repository       = Substitute.For<ICommerceRepository>();
    private readonly IAccountsAndMovementsService        _accountsService  = Substitute.For<IAccountsAndMovementsService>();
    private readonly CreateCommerceCommandHandler        _handler;

    public CreateCommerceCommandHandlerTests() =>
        _handler = new CreateCommerceCommandHandler(_repository, _accountsService);

    [Fact]
    public async Task Handle_WhenCommerceIsNew_ReturnsSuccessWithId()
    {
        _repository.ExistsByNit("900123456", default).Returns(false);
        _repository.Insert(Arg.Any<Domain.Entities.CommerceEntity>(), default).Returns(7L);
        _accountsService.CreateCommerceAccount(7, "0", Arg.Any<CancellationToken>())
            .Returns(BaseResponse<long>.Success(100L));

        var result = await _handler.Handle(
            new CreateCommerceCommand("Tienda Central", "900123456"), default);

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(ErrorCode.SUC000);
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
        _repository.DidNotReceive().Insert(Arg.Any<Domain.Entities.CommerceEntity>(), Arg.Any<CancellationToken>());
        _accountsService.DidNotReceive().CreateCommerceAccount(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertError()
    {
        _repository.ExistsByNit("900123457", default).Returns(false);
        _repository.Insert(Arg.Any<Domain.Entities.CommerceEntity>(), default).Returns(0L);

        var result = await _handler.Handle(
            new CreateCommerceCommand("Tienda Norte", "900123457"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
        _accountsService.DidNotReceive().CreateCommerceAccount(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAccountCreationFails_ReturnsAccountError()
    {
        _repository.ExistsByNit("900123457", default).Returns(false);
        _repository.Insert(Arg.Any<Domain.Entities.CommerceEntity>(), default).Returns(7L);
        _accountsService.CreateCommerceAccount(7, "0", Arg.Any<CancellationToken>())
            .Returns(BaseResponse<long>.Error(ErrorCode.ERR001, ErrorMessage.ERR001));

        var result = await _handler.Handle(
            new CreateCommerceCommand("Tienda Norte", "900123457"), default);

        result.StatusCode.Should().Be(ErrorCode.ERR001);
    }

    [Fact]
    public async Task Handle_InsertedCommerceIsActiveAndAccountUsesCommerceId()
    {
        _repository.ExistsByNit(Arg.Any<string>(), default).Returns(false);
        _repository.Insert(Arg.Any<Domain.Entities.CommerceEntity>(), default).Returns(7L);
        _accountsService.CreateCommerceAccount(7, "0", Arg.Any<CancellationToken>())
            .Returns(BaseResponse<long>.Success(100L));
        Domain.Entities.CommerceEntity? captured = null;
        await _repository.Insert(Arg.Do<Domain.Entities.CommerceEntity>(e => captured = e), default);

        var result = await _handler.Handle(new CreateCommerceCommand("Tienda", "900123458"), default);

        captured?.IsActive.Should().BeTrue();
        result.StatusCode.Should().Be(ErrorCode.SUC000);
        await _accountsService.Received(1).CreateCommerceAccount(7, "0", Arg.Any<CancellationToken>());
    }
}