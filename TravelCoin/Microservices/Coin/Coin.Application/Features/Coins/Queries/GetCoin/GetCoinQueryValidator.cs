using FluentValidation;

namespace Coin.Application.Features.Coins.Queries.GetCoin;

public class GetCoinQueryValidator : AbstractValidator<GetCoinQuery>
{
    public GetCoinQueryValidator()
    {
        RuleFor(x => x.CoinId)
            .GreaterThan(0).WithMessage("El identificador de la moneda debe ser mayor a cero.");
    }
}
