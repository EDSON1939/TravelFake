using FluentValidation;

namespace Coin.Application.Features.Coins.Commands.CreateCoin;

public class CreateCoinCommandValidator : AbstractValidator<CreateCoinCommand>
{
    public CreateCoinCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido.")
            .MaximumLength(10).WithMessage("El código no puede superar 10 caracteres.")
            .Matches("^[A-Z0-9]+$").WithMessage("El código solo puede contener letras mayúsculas y números.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("El símbolo es requerido.")
            .MaximumLength(10).WithMessage("El símbolo no puede superar 10 caracteres.");
    }
}
