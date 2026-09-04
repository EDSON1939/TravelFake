using FluentAssertions;
using NSubstitute;
using Qr.Application.Features.Qrs.Queries.GetQrByCode;
using Qr.Domain.Entities;
using Qr.Domain.Repositories;
using Xunit;

namespace Qr.Unit.Tests.Features.Qrs;

public class GetQrByCodeQueryHandlerTests
{
    private readonly IQrRepository           _repository = Substitute.For<IQrRepository>();
    private readonly GetQrByCodeQueryHandler _handler;

    public GetQrByCodeQueryHandlerTests() => _handler = new GetQrByCodeQueryHandler(_repository);

    [Fact]
    public async Task Handle_WhenQrExists_ReturnsSuccess()
    {
        var entity = new QrEntity
        {
            QrId = 1, Codigo = "QR123", ComercioId = 7, Monto = 12.50m,
            Tipo = QrTipo.UNICO, FechaExpiracion = DateTime.UtcNow.AddHours(2),
            Estado = QrEstado.ACTIVE, Activo = true, FechaCreacion = DateTime.UtcNow
        };
        _repository.GetByCode("QR123", default).Returns(entity);

        var result = await _handler.Handle(new GetQrByCodeQuery("QR123"), default);

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().NotBeNull();
        result.Data!.Codigo.Should().Be("QR123");
        result.Data.EsValido.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenQrNotFound_ReturnsError()
    {
        _repository.GetByCode("QR999", default).Returns((QrEntity?)null);

        var result = await _handler.Handle(new GetQrByCodeQuery("QR999"), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.QR_NOT_FOUND);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_TrimsCodeBeforeSearch()
    {
        _repository.GetByCode("QR123", default).Returns((QrEntity?)null);

        await _handler.Handle(new GetQrByCodeQuery("  QR123  "), default);

        await _repository.Received(1).GetByCode("QR123", default);
    }
}