using Core.Domain.Models;
using MediatR;
using Qr.Application.Common;
using Qr.Domain.Repositories;

namespace Qr.Application.Features.Qrs.Queries.GetQrById;

public class GetQrByIdQueryHandler(IQrRepository repository)
    : IRequestHandler<GetQrByIdQuery, BaseResponse<QrResponse>>
{
    public async Task<BaseResponse<QrResponse>> Handle(GetQrByIdQuery request, CancellationToken ct)
    {
        var entity = await repository.GetById(request.QrId, ct);

        if (entity is null)
            return BaseResponse<QrResponse>.Error(
                Domain.Errors.ErrorCode.QR_NOT_FOUND,
                Domain.Errors.ErrorMessage.QR_NOT_FOUND);

        return BaseResponse<QrResponse>.Success(new QrResponse(
            entity.QrId, entity.Codigo, entity.ComercioId, entity.Monto,
            entity.Tipo, entity.FechaExpiracion, entity.Estado,
            entity.Activo, entity.FechaActualizacion, entity.FechaCreacion));
    }
}