using AccountsAndMovements.Domain.Entities;
using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByOwner;

public class GetAccountByOwnerQueryValidator : AbstractValidator<GetAccountByOwnerQuery>
{
    public GetAccountByOwnerQueryValidator()
    {
        RuleFor(x => x.OwnerType)
            .NotEmpty().WithMessage("El tipo de titular es requerido.")
            .Must(x => AccountOwnerType.All.Contains(x))
            .WithMessage("El tipo de titular debe ser CLIENTE o COMERCIO.");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("El identificador del titular es requerido.");

        RuleFor(x => x.CoinCode)
            .NotEmpty().WithMessage("El código de la moneda es requerido.")
            .MaximumLength(10).WithMessage("El código no puede superar 10 caracteres.");
    }
}
