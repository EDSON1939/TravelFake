using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateMerchantAccount;

public class CreateMerchantAccountCommandValidator : AbstractValidator<CreateMerchantAccountCommand>
{
    public CreateMerchantAccountCommandValidator()
    {
        RuleFor(x => x.MerchantId)
            .GreaterThan(0).WithMessage("El identificador del comercio es requerido.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
    }
}
