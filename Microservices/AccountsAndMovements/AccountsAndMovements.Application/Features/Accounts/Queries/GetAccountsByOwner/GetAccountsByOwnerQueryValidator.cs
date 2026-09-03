using AccountsAndMovements.Domain.Entities;
using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByOwner;

public class GetAccountsByOwnerQueryValidator : AbstractValidator<GetAccountsByOwnerQuery>
{
    public GetAccountsByOwnerQueryValidator()
    {
        RuleFor(x => x.OwnerType)
            .NotEmpty().WithMessage("El tipo de titular es requerido.")
            .Must(x => AccountOwnerType.All.Contains(x))
            .WithMessage("El tipo de titular debe ser CLIENTE o COMERCIO.");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("El identificador del titular es requerido.");
    }
}
