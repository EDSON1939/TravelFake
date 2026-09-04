using Core.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Qr.Application.Features.Qrs.Commands.CreateQr;
using Qr.Domain.Interfaces;
using Qr.Domain.Repositories;
using Xunit;

namespace Qr.Unit.Tests.Features.Qrs;

public class CreateQrCommandHandlerTests
{
    private readonly IQrRepository        _repository       = Substitute.For<IQrRepository>();
    private readonly ICommerceService     _commerceService  = Substitute.For<ICommerceService>();
    private readonly CreateQrCommandHandler _handler;

    public CreateQrCommandHandlerTests() =>
        _handler = new CreateQrCommandHandler(_repository, _commerceService);

    [Fact]
    public async Task Handle_WhenCommerceExists_ReturnsSuccessWithId()
    {
        _commerceService.ExistsAsync(7, Arg.Any<CancellationToken>())
            .Returns(BaseResponse<bool>.Success(true));
        _repository.GetByCode(Arg.Any<string>(), default).Returns((Domain.Entities.QrEntity?)null);
        _repository.Insert(Arg.Any<Domain.Entities.QrEntity>(), default).Returns(9L);

        var result = await _handler.Handle(
            new CreateQrCommand(7, 12.50m, "UNICO", DateTime.UtcNow.AddHours(2)), default);

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(9L);
    }

    [Fact]
    public async Task Handle_WhenCodeCollides_RetriesWithNewCode()
    {
        _commerceService.ExistsAsync(7, Arg.Any<CancellationToken>())
            .Returns(BaseResponse<bool>.Success(true));
        _repository.GetByCode(Arg.Any<string>(), default)
            .Returns((Domain.Entities.QrEntity?)new(),
                (Domain.Entities.QrEntity?)null);
        _repository.Insert(Arg.Any<Domain.Entities.QrEntity>(), default).Returns(9L);

        var result = await _handler.Handle(
            new CreateQrCommand(7, 12.50m, "UNICO", DateTime.UtcNow.AddHours(2)), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(9L);
        await _repository.Received(2).GetByCode(Arg.Any<string>(), default);
    }

    [Fact]
    public async Task Handle_WhenCommerceDoesNotExist_ReturnsCommerceError()
    {
        _commerceService.ExistsAsync(999, Arg.Any<CancellationToken>())
            .Returns(BaseResponse<bool>.Error(Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND,
                Domain.Errors.ErrorMessage.COMMERCE_NOT_FOUND));

        var result = await _handler.Handle(
            new CreateQrCommand(999, 12.50m, "UNICO", DateTime.UtcNow.AddHours(2)), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND);
        result.Data.Should().Be(0);
        await _repository.DidNotReceive().Insert(Arg.Any<Domain.Entities.QrEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertError()
    {
        _commerceService.ExistsAsync(7, Arg.Any<CancellationToken>())
            .Returns(BaseResponse<bool>.Success(true));
        _repository.GetByCode(Arg.Any<string>(), default).Returns((Domain.Entities.QrEntity?)null);
        _repository.Insert(Arg.Any<Domain.Entities.QrEntity>(), default).Returns(0L);

        var result = await _handler.Handle(
            new CreateQrCommand(7, 12.50m, "MULTIPLE", DateTime.UtcNow.AddHours(2)), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
    }

    [Fact]
    public async Task Handle_GeneratedCodeIsNotEmptyAndInserted()
    {
        _commerceService.ExistsAsync(7, Arg.Any<CancellationToken>())
            .Returns(BaseResponse<bool>.Success(true));
        _repository.GetByCode(Arg.Any<string>(), default).Returns((Domain.Entities.QrEntity?)null);
        _repository.Insert(Arg.Any<Domain.Entities.QrEntity>(), default).Returns(9L);
        Domain.Entities.QrEntity? captured = null;
        await _repository.Insert(Arg.Do<Domain.Entities.QrEntity>(e => captured = e), default);

        var result = await _handler.Handle(
            new CreateQrCommand(7, 12.50m, "UNICO", DateTime.UtcNow.AddHours(2)), default);

        captured.Should().NotBeNull();
        captured!.Codigo.Should().NotBeNullOrEmpty();
        captured.ComercioId.Should().Be(7);
        captured.Monto.Should().Be(12.50m);
        captured.Estado.Should().Be(Domain.Entities.QrEstado.ACTIVE);
        captured.Activo.Should().BeTrue();
        captured.Tipo.Should().Be("UNICO");
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
    }
}