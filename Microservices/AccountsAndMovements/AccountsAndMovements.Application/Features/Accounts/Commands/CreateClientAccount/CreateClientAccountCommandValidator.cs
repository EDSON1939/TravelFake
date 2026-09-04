using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateClientAccount;

public class CreateClientAccountCommandValidator : AbstractValidator<CreateClientAccountCommand>
{
    public CreateClientAccountCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("El identificador del cliente es requerido.");

        RuleFor(x => x.CoinCode)
            .NotEmpty().WithMessage("El código de la moneda es requerido.")
            .MaximumLength(10).WithMessage("El código no puede superar 10 caracteres.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("El código solo puede contener letras y números.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
    }
}
