using FluentValidation;

namespace AccountsAndMovements.Application.Features.Payments.Queries.GetPayment;

public class GetPaymentQueryValidator : AbstractValidator<GetPaymentQuery>
{
    public GetPaymentQueryValidator()
    {
        RuleFor(x => x.TransactionCode)
            .MaximumLength(36).WithMessage("El código de operación no puede superar 36 caracteres.");

        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(64).WithMessage("La clave de idempotencia no puede superar 64 caracteres.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.TransactionCode)
                    || !string.IsNullOrWhiteSpace(x.IdempotencyKey))
            .WithMessage("Debe enviar el código de operación o la clave de idempotencia.");
    }
}
