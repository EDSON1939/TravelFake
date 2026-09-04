using FluentValidation;

namespace Commerce.Application.Features.Commerces.Queries.GetCommerceList;

public class GetCommerceListQueryValidator : AbstractValidator<GetCommerceListQuery>
{
    public GetCommerceListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("El número de página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
    }
}
