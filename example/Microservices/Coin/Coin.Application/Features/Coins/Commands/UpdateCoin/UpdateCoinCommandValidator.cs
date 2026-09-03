using FluentValidation;

namespace Coin.Application.Features.Coins.Commands.UpdateCoin;

public class UpdateCoinCommandValidator : AbstractValidator<UpdateCoinCommand>
{
    public UpdateCoinCommandValidator()
    {
        RuleFor(x => x.CoinId)
            .GreaterThan(0).WithMessage("El identificador de la moneda debe ser mayor a cero.");
    }
}
