using FluentValidation;

namespace Qr.Application.Features.Qrs.Queries.GetQrById;

public class GetQrByIdQueryValidator : AbstractValidator<GetQrByIdQuery>
{
    public GetQrByIdQueryValidator()
    {
        RuleFor(x => x.QrId)
            .GreaterThan(0).WithMessage("El identificador del QR debe ser mayor a cero.");
    }
}