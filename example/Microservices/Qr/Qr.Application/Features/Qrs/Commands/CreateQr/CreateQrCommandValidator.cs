using FluentValidation;
using Qr.Domain.Entities;

namespace Qr.Application.Features.Qrs.Commands.CreateQr;

public class CreateQrCommandValidator : AbstractValidator<CreateQrCommand>
{
    public CreateQrCommandValidator()
    {
        RuleFor(x => x.ComercioId)
            .GreaterThan(0).WithMessage("El identificador del comercio debe ser mayor a cero.");

        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto del QR debe ser mayor a cero.");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo del QR es requerido.")
            .Must(x => x == QrTipo.UNICO || x == QrTipo.MULTIPLE)
            .WithMessage("El tipo del QR debe ser UNICO o MULTIPLE.");

        RuleFor(x => x.FechaExpiracion)
            .NotEmpty().WithMessage("La fecha de expiración es requerida.")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha de expiración debe ser posterior a la fecha actual.");
    }
}