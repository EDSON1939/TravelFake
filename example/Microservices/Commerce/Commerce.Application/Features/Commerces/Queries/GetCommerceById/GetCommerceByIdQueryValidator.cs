using FluentValidation;

namespace Commerce.Application.Features.Commerces.Queries.GetCommerceById;

public class GetCommerceByIdQueryValidator : AbstractValidator<GetCommerceByIdQuery>
{
    public GetCommerceByIdQueryValidator()
    {
        RuleFor(x => x.CommerceId)
            .GreaterThan(0).WithMessage("El identificador del comercio debe ser mayor a cero.");
    }
}
