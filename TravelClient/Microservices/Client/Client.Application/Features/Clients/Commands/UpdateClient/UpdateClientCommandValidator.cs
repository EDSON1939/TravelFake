using FluentValidation;

namespace Client.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("El identificador del cliente debe ser mayor a cero.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .MaximumLength(100).WithMessage("El correo electrónico no puede superar 100 caracteres.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es requerido.")
            .MaximumLength(100).WithMessage("El teléfono no puede superar 100 caracteres.")
            .Matches(@"^[0-9+()\-\s]+$").WithMessage("El teléfono solo puede contener números y los símbolos + ( ) -.");

        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("El identificador del país debe ser mayor a cero.");
    }
}
