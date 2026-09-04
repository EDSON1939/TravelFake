using FluentValidation;

namespace Client.Application.Features.Clients.Queries.GetClient;

public class GetClientQueryValidator : AbstractValidator<GetClientQuery>
{
    public GetClientQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("El identificador del cliente debe ser mayor a cero.");
    }
}
