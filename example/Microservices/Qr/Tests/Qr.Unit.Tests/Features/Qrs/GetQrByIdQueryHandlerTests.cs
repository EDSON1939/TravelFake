using FluentAssertions;
using NSubstitute;
using Qr.Application.Features.Qrs.Queries.GetQrById;
using Qr.Domain.Entities;
using Qr.Domain.Repositories;
using Xunit;

namespace Qr.Unit.Tests.Features.Qrs;

public class GetQrByIdQueryHandlerTests
{
    private readonly IQrRepository          _repository = Substitute.For<IQrRepository>();
    private readonly GetQrByIdQueryHandler  _handler;

    public GetQrByIdQueryHandlerTests() => _handler = new GetQrByIdQueryHandler(_repository);

    [Fact]
    public async Task Handle_WhenQrExistsAndIsActive_ReturnsValidQr()
    {
        var entity = new QrEntity
        {
            QrId = 1, Codigo = "QR123", ComercioId = 7, Monto = 12.50m,
            Tipo = QrTipo.UNICO, FechaExpiracion = DateTime.UtcNow.AddHours(2),
            Estado = QrEstado.ACTIVE, Activo = true, FechaCreacion = DateTime.UtcNow
        };
        _repository.GetById(1, default).Returns(entity);

        var result = await _handler.Handle(new GetQrByIdQuery(1), default);

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().NotBeNull();
        result.Data!.Codigo.Should().Be("QR123");
        result.Data.ComercioId.Should().Be(7);
        result.Data.Monto.Should().Be(12.50m);
        result.Data.Estado.Should().Be(QrEstado.ACTIVE);
        result.Data.EsValido.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenQrIsUsed_ReturnsNotValid()
    {
        var entity = new QrEntity
        {
            QrId = 1, Codigo = "QR123", ComercioId = 7, Monto = 12.50m,
            Tipo = QrTipo.UNICO, FechaExpiracion = DateTime.UtcNow.AddHours(2),
            Estado = QrEstado.USED, Activo = true, FechaCreacion = DateTime.UtcNow
        };
        _repository.GetById(1, default).Returns(entity);

        var result = await _handler.Handle(new GetQrByIdQuery(1), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.EsValido.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenQrIsExpired_ReturnsNotValid()
    {
        var entity = new QrEntity
        {
            QrId = 1, Codigo = "QR123", ComercioId = 7, Monto = 12.50m,
            Tipo = QrTipo.MULTIPLE, FechaExpiracion = DateTime.UtcNow.AddHours(2),
            Estado = QrEstado.EXPIRED, Activo = true, FechaCreacion = DateTime.UtcNow
        };
        _repository.GetById(1, default).Returns(entity);

        var result = await _handler.Handle(new GetQrByIdQuery(1), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.EsValido.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenQrNotFound_ReturnsError()
    {
        _repository.GetById(999, default).Returns((QrEntity?)null);

        var result = await _handler.Handle(new GetQrByIdQuery(999), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.QR_NOT_FOUND);
        result.Data.Should().BeNull();
    }
}