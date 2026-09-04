using FluentValidation;

namespace Country.Application.Features.Countries.Queries.GetCountry;

public class GetCountryQueryValidator : AbstractValidator<GetCountryQuery>
{
    public GetCountryQueryValidator()
    {
        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("El identificador del país debe ser mayor a cero.");
    }
}
