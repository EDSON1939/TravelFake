using Auth.Domain.Entities;
using FluentValidation;

namespace Auth.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El usuario es requerido.")
            .MaximumLength(50).WithMessage("El usuario no puede superar 50 caracteres.")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("El usuario solo puede contener letras, números, punto, guion y guion bajo.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128).WithMessage("La contraseña no puede superar 128 caracteres.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es requerido.")
            .Must(x => UserRole.All.Contains(x))
            .WithMessage("El rol debe ser CLIENTE, AGENTE o ADMIN.");

        RuleFor(x => x.Document)
            .MaximumLength(20).WithMessage("El documento no puede superar 20 caracteres.");

        // Un CLIENTE o AGENTE representa a alguien del negocio: sin documento no
        // hay a quién vincularlo. Solo ADMIN puede existir sin cliente.
        RuleFor(x => x.Document)
            .NotEmpty().When(x => x.Role != UserRole.ADMIN)
            .WithMessage("El documento del cliente es requerido para los roles CLIENTE y AGENTE.");
    }
}
