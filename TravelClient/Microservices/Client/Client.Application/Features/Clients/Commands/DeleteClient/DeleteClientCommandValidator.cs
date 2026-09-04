using FluentValidation;

namespace Client.Application.Features.Clients.Commands.DeleteClient;

public class DeleteClientCommandValidator : AbstractValidator<DeleteClientCommand>
{
    public DeleteClientCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("El identificador del cliente debe ser mayor a cero.");
    }
}
