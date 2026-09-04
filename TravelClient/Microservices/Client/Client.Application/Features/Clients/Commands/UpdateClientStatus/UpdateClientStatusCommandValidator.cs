using FluentValidation;

namespace Client.Application.Features.Clients.Commands.UpdateClientStatus;

public class UpdateClientStatusCommandValidator : AbstractValidator<UpdateClientStatusCommand>
{
    public UpdateClientStatusCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("El identificador del cliente debe ser mayor a cero.");
    }
}
