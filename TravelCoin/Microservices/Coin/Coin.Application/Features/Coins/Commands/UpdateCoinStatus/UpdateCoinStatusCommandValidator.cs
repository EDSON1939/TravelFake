using FluentValidation;

namespace Coin.Application.Features.Coins.Commands.UpdateCoinStatus;

public class UpdateCoinStatusCommandValidator : AbstractValidator<UpdateCoinStatusCommand>
{
    public UpdateCoinStatusCommandValidator()
    {
        RuleFor(x => x.CoinId)
            .GreaterThan(0).WithMessage("El identificador de la moneda debe ser mayor a cero.");
    }
}
