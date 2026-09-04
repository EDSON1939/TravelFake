using FluentValidation;

namespace Commerce.Application.Features.Commerces.Commands.CreateCommerce;

public class CreateCommerceCommandValidator : AbstractValidator<CreateCommerceCommand>
{
    public CreateCommerceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.")
            .MaximumLength(50).WithMessage("El nombre no puede superar 50 caracteres.");

        RuleFor(x => x.Nit)
            .NotEmpty().WithMessage("El NIT es requerido.")
            .MinimumLength(5).WithMessage("El NIT debe tener al menos 5 caracteres.")
            .MaximumLength(13).WithMessage("El NIT no puede superar 13 caracteres.")
            .Matches("^[0-9-]+$").WithMessage("El NIT solo puede contener números y guiones.");
    }
}
