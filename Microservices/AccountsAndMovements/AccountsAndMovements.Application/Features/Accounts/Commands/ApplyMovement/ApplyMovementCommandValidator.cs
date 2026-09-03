using AccountsAndMovements.Domain.Entities;
using FluentValidation;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.ApplyMovement;

public class ApplyMovementCommandValidator : AbstractValidator<ApplyMovementCommand>
{
    public ApplyMovementCommandValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("El número de cuenta es requerido.")
            .MaximumLength(20).WithMessage("El número de cuenta no puede superar 20 caracteres.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de movimiento es requerido.")
            .Must(x => MovementType.All.Contains(x))
            .WithMessage("El tipo de movimiento debe ser CREDITO o DEBITO.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("La clave de idempotencia es requerida.")
            .MaximumLength(60).WithMessage("La clave de idempotencia no puede superar 60 caracteres.");

        RuleFor(x => x.Reference)
            .MaximumLength(64).WithMessage("La referencia no puede superar 64 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("La descripción no puede superar 250 caracteres.");
    }
}
