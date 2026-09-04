using FluentValidation;

namespace Commerce.Application.Features.Commerces.Commands.UpdateStatus;

public class UpdateCommerceStatusCommandValidator : AbstractValidator<UpdateCommerceStatusCommand>
{
    public UpdateCommerceStatusCommandValidator()
    {
        RuleFor(x => x.CommerceId)
            .GreaterThan(0).WithMessage("El identificador del comercio debe ser mayor a cero.");
    }
}
