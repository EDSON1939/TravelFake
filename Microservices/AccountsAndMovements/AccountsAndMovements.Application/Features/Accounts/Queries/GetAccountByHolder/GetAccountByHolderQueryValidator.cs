using AccountsAndMovements.Domain.Entities;
using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByHolder;

public class GetAccountByHolderQueryValidator : AbstractValidator<GetAccountByHolderQuery>
{
    public GetAccountByHolderQueryValidator()
    {
        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("El tipo de titular es requerido.")
            .Must(x => AccountType.All.Contains(x))
            .WithMessage("El tipo de cuenta debe ser CLIENT o COMMERCE.");

        RuleFor(x => x.HolderId)
            .GreaterThan(0).WithMessage("El identificador del titular es requerido.");

        RuleFor(x => x.CoinCode)
            .NotEmpty().WithMessage("El código de la moneda es requerido.")
            .MaximumLength(10).WithMessage("El código no puede superar 10 caracteres.");
    }
}
