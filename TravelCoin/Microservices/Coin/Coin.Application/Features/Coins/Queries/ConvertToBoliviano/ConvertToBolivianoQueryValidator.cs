using Coin.Application.Common;
using FluentValidation;

namespace Coin.Application.Features.Coins.Queries.ConvertToBoliviano;

public class ConvertToBolivianoQueryValidator : AbstractValidator<ConvertToBolivianoQuery>
{
    public ConvertToBolivianoQueryValidator()
    {
        RuleFor(x => x.CoinId)
            .GreaterThan(0).WithMessage("El identificador de la moneda debe ser mayor a cero.");

        RuleFor(x => x.Amount)
            .Cascade(CascadeMode.Stop)
            .MustBeMoney("El monto a convertir");
    }
}
