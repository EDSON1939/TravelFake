using FluentValidation;

namespace Coin.Application.Features.Coins.Commands.DeleteCoin;

public class DeleteCoinCommandValidator : AbstractValidator<DeleteCoinCommand>
{
    public DeleteCoinCommandValidator()
    {
        RuleFor(x => x.CoinId)
            .GreaterThan(0).WithMessage("El identificador de la moneda debe ser mayor a cero.");
    }
}
