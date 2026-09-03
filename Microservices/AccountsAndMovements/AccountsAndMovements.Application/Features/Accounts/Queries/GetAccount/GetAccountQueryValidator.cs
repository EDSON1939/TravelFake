using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccount;

public class GetAccountQueryValidator : AbstractValidator<GetAccountQuery>
{
    public GetAccountQueryValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("El número de cuenta es requerido.")
            .MaximumLength(20).WithMessage("El número de cuenta no puede superar 20 caracteres.");
    }
}
