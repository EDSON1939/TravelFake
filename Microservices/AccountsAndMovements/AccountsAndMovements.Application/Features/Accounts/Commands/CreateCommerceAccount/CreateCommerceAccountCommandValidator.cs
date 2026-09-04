using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateCommerceAccount;

public class CreateCommerceAccountCommandValidator : AbstractValidator<CreateCommerceAccountCommand>
{
    public CreateCommerceAccountCommandValidator()
    {
        RuleFor(x => x.CommerceId)
            .GreaterThan(0).WithMessage("El identificador del comercio es requerido.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
    }
}
