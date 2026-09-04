using FluentAssertions;
using NSubstitute;
using Qr.Application.Features.Qrs.Commands.ConsumeQr;
using Qr.Domain.Entities;
using Qr.Domain.Repositories;
using Xunit;

namespace Qr.Unit.Tests.Features.Qrs;

public class ConsumeQrCommandHandlerTests
{
    private readonly IQrRepository          _repository = Substitute.For<IQrRepository>();
    private readonly ConsumeQrCommandHandler _handler;

    public ConsumeQrCommandHandlerTests() => _handler = new ConsumeQrCommandHandler(_repository);

    private static QrEntity ActiveQr(string tipo) => new()
    {
        QrId = 1,
        Codigo = "QR123",
        ComercioId = 7,
        Monto = 12.50m,
        Tipo = tipo,
        FechaExpiracion = DateTime.UtcNow.AddHours(2),
        Estado = QrEstado.ACTIVE,
        Activo = true,
        FechaCreacion = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_WhenSingleUseQrIsActive_MarksItAsUsed()
    {
        _repository.GetByCode("QR123", default).Returns(ActiveQr(QrTipo.UNICO));
        _repository.MarkUsed(1, default).Returns(1L);

        var result = await _handler.Handle(new ConsumeQrCommand("QR123"), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(1);
        await _repository.Received(1).MarkUsed(1, default);
    }

    [Fact]
    public async Task Handle_WhenMultiUseQrIsActive_DoesNotMarkUsed()
    {
        _repository.GetByCode("QR123", default).Returns(ActiveQr(QrTipo.MULTIPLE));

        var result = await _handler.Handle(new ConsumeQrCommand("QR123"), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(1);
        await _repository.DidNotReceive().MarkUsed(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQrNotFound_ReturnsNotFound()
    {
        _repository.GetByCode("QR999", default).Returns((QrEntity?)null);

        var result = await _handler.Handle(new ConsumeQrCommand("QR999"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.QR_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenQrIsExpired_ReturnsExpiredError()
    {
        var qr = ActiveQr(QrTipo.UNICO);
        qr.Estado = QrEstado.EXPIRED;
        _repository.GetByCode("QR123", default).Returns(qr);

        var result = await _handler.Handle(new ConsumeQrCommand("QR123"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.QR_EXPIRED);
        await _repository.DidNotReceive().MarkUsed(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQrAlreadyUsed_ReturnsUsedError()
    {
        var qr = ActiveQr(QrTipo.UNICO);
        qr.Estado = QrEstado.USED;
        _repository.GetByCode("QR123", default).Returns(qr);

        var result = await _handler.Handle(new ConsumeQrCommand("QR123"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.QR_USED);
        await _repository.DidNotReceive().MarkUsed(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQrIsInactive_ReturnsNotActiveError()
    {
        var qr = ActiveQr(QrTipo.UNICO);
        qr.Activo = false;
        _repository.GetByCode("QR123", default).Returns(qr);

        var result = await _handler.Handle(new ConsumeQrCommand("QR123"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.QR_NOT_ACTIVE);
    }
}