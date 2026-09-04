using FluentAssertions;
using Qr.Application.Features.Qrs.Commands.CreateQr;
using Xunit;

namespace Qr.Unit.Tests.Features.Qrs;

public class CreateQrCommandValidatorTests
{
    private readonly CreateQrCommandValidator _validator = new();

    [Theory]
    [InlineData(Qr.Domain.Entities.QrTipo.UNICO)]
    [InlineData(Qr.Domain.Entities.QrTipo.MULTIPLE)]
    public async Task Validate_WhenCommandIsValid_PassesValidation(string tipo)
    {
        var result = await _validator.ValidateAsync(
            new CreateQrCommand(1, 12.50m, tipo, DateTime.UtcNow.AddHours(2)));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenComercioIdIsInvalid_FailsValidation(long comercioId)
    {
        var result = await _validator.ValidateAsync(
            new CreateQrCommand(comercioId, 12.50m, Qr.Domain.Entities.QrTipo.UNICO, DateTime.UtcNow.AddHours(2)));
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5.5)]
    public async Task Validate_WhenMontoIsInvalid_FailsValidation(decimal monto)
    {
        var result = await _validator.ValidateAsync(
            new CreateQrCommand(1, monto, Qr.Domain.Entities.QrTipo.UNICO, DateTime.UtcNow.AddHours(2)));
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UNO")]
    public async Task Validate_WhenTipoIsInvalid_FailsValidation(string tipo)
    {
        var result = await _validator.ValidateAsync(
            new CreateQrCommand(1, 12.50m, tipo, DateTime.UtcNow.AddHours(2)));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenFechaExpiracionIsInThePast_FailsValidation()
    {
        var result = await _validator.ValidateAsync(
            new CreateQrCommand(1, 12.50m, Qr.Domain.Entities.QrTipo.UNICO, DateTime.UtcNow.AddHours(-1)));
        result.IsValid.Should().BeFalse();
    }
}