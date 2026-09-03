using FluentValidation;

namespace AccountsAndMovements.Application.Features.Movements.Queries.GetMovements;

public class GetMovementsQueryValidator : AbstractValidator<GetMovementsQuery>
{
    public GetMovementsQueryValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("El número de cuenta es requerido.")
            .MaximumLength(20).WithMessage("El número de cuenta no puede superar 20 caracteres.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("La página debe ser mayor a cero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
    }
}
