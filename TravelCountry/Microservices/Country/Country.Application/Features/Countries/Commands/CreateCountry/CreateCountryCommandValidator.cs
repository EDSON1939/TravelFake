using FluentValidation;

namespace Country.Application.Features.Countries.Commands.CreateCountry;

public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido.")
            .MaximumLength(100).WithMessage("El código no puede superar 100 caracteres.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("El código solo puede contener letras y números.");
    }
}
