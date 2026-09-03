using FluentValidation;

namespace AccountsAndMovements.Application.Features.Payments.Commands.ExecuteQrPayment;

public class ExecuteQrPaymentCommandValidator : AbstractValidator<ExecuteQrPaymentCommand>
{
    public ExecuteQrPaymentCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("El identificador del cliente es requerido.");

        RuleFor(x => x.QrCode)
            .NotEmpty().WithMessage("El código QR es requerido.")
            .MaximumLength(64).WithMessage("El código QR no puede superar 64 caracteres.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .MaximumLength(10).WithMessage("La moneda no puede superar 10 caracteres.")
            .Matches("^[A-Za-z]{3,10}$").WithMessage("La moneda debe ser un código alfabético (ej: USD).");

        // 60 y no 64: el asiento del comercio reutiliza esta clave con el sufijo
        // "-IN", y la columna admite 64 caracteres.
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("La clave de idempotencia es requerida.")
            .MaximumLength(60).WithMessage("La clave de idempotencia no puede superar 60 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("La descripción no puede superar 250 caracteres.");
    }
}
