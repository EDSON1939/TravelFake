using Core.Domain.Models;
using MediatR;
using Qr.Application.Common;
using Qr.Domain.Repositories;

namespace Qr.Application.Features.Qrs.Queries.GetQrByCode;

public class GetQrByCodeQueryHandler(IQrRepository repository)
    : IRequestHandler<GetQrByCodeQuery, BaseResponse<QrResponse>>
{
    public async Task<BaseResponse<QrResponse>> Handle(GetQrByCodeQuery request, CancellationToken ct)
    {
        var entity = await repository.GetByCode(request.Code.Trim(), ct);

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