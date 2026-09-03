using AccountsAndMovements.Domain.Entities;
using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.OwnerType)
            .NotEmpty().WithMessage("El tipo de titular es requerido.")
            .Must(x => AccountOwnerType.All.Contains(x))
            .WithMessage("El tipo de titular debe ser CLIENTE o COMERCIO.");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("El identificador del titular es requerido.");

        RuleFor(x => x.CoinCode)
            .NotEmpty().WithMessage("El código de la moneda es requerido.")
            .MaximumLength(10).WithMessage("El código no puede superar 10 caracteres.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("El código solo puede contener letras y números.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
    }
}
