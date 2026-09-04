using FluentValidation;

namespace Country.Application.Features.Countries.Commands.UpdateCountryStatus;

public class UpdateCountryStatusCommandValidator : AbstractValidator<UpdateCountryStatusCommand>
{
    public UpdateCountryStatusCommandValidator()
    {
        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("El identificador del país debe ser mayor a cero.");
    }
}
