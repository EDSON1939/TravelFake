using FluentValidation;

namespace Auth.Application.Features.Users.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El usuario es requerido.")
            .MaximumLength(50).WithMessage("El usuario no puede superar 50 caracteres.");

        // Sin MaximumLength baja: un límite corto aquí es una debilidad, no una
        // validación. Solo evitamos payloads absurdos.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MaximumLength(128).WithMessage("La contraseña no puede superar 128 caracteres.");
    }
}
