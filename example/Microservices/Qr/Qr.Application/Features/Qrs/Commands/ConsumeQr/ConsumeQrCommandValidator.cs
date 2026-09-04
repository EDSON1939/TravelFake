using FluentValidation;

namespace Qr.Application.Features.Qrs.Commands.ConsumeQr;

public class ConsumeQrCommandValidator : AbstractValidator<ConsumeQrCommand>
{
    public ConsumeQrCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código del QR es requerido.")
            .MaximumLength(40).WithMessage("El código no puede superar 40 caracteres.")
            .Matches("^[A-Z0-9]+$").WithMessage("El código solo puede contener letras mayúsculas y números.");
    }
}