using FluentValidation;

namespace Country.Application.Features.Countries.Commands.UpdateCountry;

public class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryCommandValidator()
    {
        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("El identificador del país debe ser mayor a cero.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido.")
            .MaximumLength(100).WithMessage("El código no puede superar 100 caracteres.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("El código solo puede contener letras y números.");
    }
}
