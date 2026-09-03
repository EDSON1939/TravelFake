using AccountsAndMovements.Application.Features.Payments.Commands.ExecuteQrPayment;
using FluentAssertions;
using Xunit;

namespace AccountsAndMovements.Unit.Tests.Features.Payments;

public class ExecuteQrPaymentCommandValidatorTests
{
    private readonly ExecuteQrPaymentCommandValidator _validator = new();

    private static ExecuteQrPaymentCommand Command(
        long    clientId       = 7,
        string  qrCode         = "QR-BO-0001",
        decimal amount         = 20m,
        string  currency       = "USD",
        string  idempotencyKey = "TX-2026-000001")
        => new(clientId, qrCode, amount, currency, idempotencyKey, "Pago QR");

    [Fact]
    public void Validate_WhenTheRequestIsComplete_Passes()
    {
        _validator.Validate(Command()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_WhenTheAmountIsNotPositive_Fails(decimal amount)
    {
        var result = _validator.Validate(Command(amount: amount));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ExecuteQrPaymentCommand.Amount));
    }

    [Fact]
    public void Validate_WhenTheIdempotencyKeyIsMissing_Fails()
    {
        var result = _validator.Validate(Command(idempotencyKey: ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ExecuteQrPaymentCommand.IdempotencyKey));
    }

    [Fact]
    public void Validate_WhenTheIdempotencyKeyIsTooLongForTheCreditLeg_Fails()
    {
        // El asiento del comercio reutiliza la clave con el sufijo "-IN" y la
        // columna admite 64 caracteres: por eso el tope son 60.
        var result = _validator.Validate(Command(idempotencyKey: new string('K', 61)));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenTheQrCodeIsMissing_Fails()
    {
        _validator.Validate(Command(qrCode: "")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenTheClientIsNotIdentified_Fails()
    {
        _validator.Validate(Command(clientId: 0)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("US")]
    [InlineData("US1")]
    [InlineData("")]
    public void Validate_WhenTheCurrencyIsNotAnAlphabeticCode_Fails(string currency)
    {
        _validator.Validate(Command(currency: currency)).IsValid.Should().BeFalse();
    }
}
