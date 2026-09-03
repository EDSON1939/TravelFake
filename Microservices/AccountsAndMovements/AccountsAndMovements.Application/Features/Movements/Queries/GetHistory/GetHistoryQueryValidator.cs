using AccountsAndMovements.Domain.Entities;
using FluentValidation;

namespace AccountsAndMovements.Application.Features.Movements.Queries.GetHistory;

public class GetHistoryQueryValidator : AbstractValidator<GetHistoryQuery>
{
    public GetHistoryQueryValidator()
    {
        RuleFor(x => x.OwnerType)
            .NotEmpty().WithMessage("El tipo de titular es requerido.")
            .Must(x => AccountOwnerType.All.Contains(x))
            .WithMessage("El tipo de titular debe ser CLIENTE o COMERCIO.");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("El identificador del titular es requerido.");

        RuleFor(x => x.Status)
            .Must(x => string.IsNullOrEmpty(x) || MovementStatus.All.Contains(x))
            .WithMessage("El estado debe ser PENDING, COMPLETED, FAILED o CANCELLED.");

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom!.Value)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("La fecha final no puede ser anterior a la inicial.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("La página debe ser mayor a cero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
    }
}
