using FluentValidation;

namespace Country.Application.Features.Countries.Commands.DeleteCountry;

public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommand>
{
    public DeleteCountryCommandValidator()
    {
        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("El identificador del país debe ser mayor a cero.");
    }
}
