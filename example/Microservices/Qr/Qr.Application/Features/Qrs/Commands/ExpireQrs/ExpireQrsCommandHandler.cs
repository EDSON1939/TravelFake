using Core.Domain.Models;
using MediatR;
using Qr.Domain.Repositories;

namespace Qr.Application.Features.Qrs.Commands.ExpireQrs;

public class ExpireQrsCommandHandler(IQrRepository repository)
    : IRequestHandler<ExpireQrsCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(ExpireQrsCommand request, CancellationToken ct)
    {
        // Marca como EXPIRED todo QR con estado ACTIVO cuya fecha de expiración
        // ya venció. Lo invoca el servicio en background cada hora.
        var affected = await repository.MarkExpired(ct);

        return BaseResponse<long>.Success(affected);
    }
}