using FluentValidation;

namespace Coin.Application.Features.Coins.Queries.GetCoin;

public class GetCoinQueryValidator : AbstractValidator<GetCoinQuery>
{
    public GetCoinQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de la moneda es requerido.")
            .MaximumLength(10).WithMessage("El código no puede superar 10 caracteres.")
            .Matches("^[A-Z0-9]+$").WithMessage("El código solo puede contener letras mayúsculas y números.");
    }
}
