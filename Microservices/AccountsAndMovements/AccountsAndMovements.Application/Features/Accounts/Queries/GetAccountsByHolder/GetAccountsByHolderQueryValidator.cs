using AccountsAndMovements.Domain.Entities;
using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByHolder;

public class GetAccountsByHolderQueryValidator : AbstractValidator<GetAccountsByHolderQuery>
{
    public GetAccountsByHolderQueryValidator()
    {
        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("El tipo de titular es requerido.")
            .Must(x => AccountType.All.Contains(x))
            .WithMessage("El tipo de cuenta debe ser CLIENT o COMMERCE.");

        RuleFor(x => x.HolderId)
            .GreaterThan(0).WithMessage("El identificador del titular es requerido.");
    }
}
