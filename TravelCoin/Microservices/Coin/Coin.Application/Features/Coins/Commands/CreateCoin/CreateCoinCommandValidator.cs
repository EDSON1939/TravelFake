using Coin.Application.Common;
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
            .MaximumLength(100).WithMessage("El código no puede superar 100 caracteres.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("El código solo puede contener letras y números.");

        RuleFor(x => x.BuyRate)
            .Cascade(CascadeMode.Stop)
            .MustBeMoney("El tipo de cambio de compra");
    }
}
