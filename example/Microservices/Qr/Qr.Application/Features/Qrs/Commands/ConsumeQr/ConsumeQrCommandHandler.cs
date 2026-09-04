using Core.Domain.Models;
using MediatR;
using Qr.Domain.Entities;
using Qr.Domain.Repositories;

namespace Qr.Application.Features.Qrs.Commands.ConsumeQr;

public class ConsumeQrCommandHandler(IQrRepository repository)
    : IRequestHandler<ConsumeQrCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(ConsumeQrCommand request, CancellationToken ct)
    {
        var qr = await repository.GetByCode(request.Code.Trim(), ct);

        if (qr is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.QR_NOT_FOUND,
                Domain.Errors.ErrorMessage.QR_NOT_FOUND);

        if (qr.Estado == QrEstado.EXPIRED)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.QR_EXPIRED,
                Domain.Errors.ErrorMessage.QR_EXPIRED);

        if (qr.Estado == QrEstado.USED)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.QR_USED,
                Domain.Errors.ErrorMessage.QR_USED);

        if (qr.Estado != QrEstado.ACTIVE || !qr.Activo)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.QR_NOT_ACTIVE,
                Domain.Errors.ErrorMessage.QR_NOT_ACTIVE);

        // Solo los QR de unico uso pasan de ACTIVO a USED; los de uso multiple
        // permanecen ACTIVOS hasta expirar.
        if (qr.Tipo == QrTipo.UNICO)
        {
            var affected = await repository.MarkUsed(qr.QrId, ct);
            if (affected <= 0)
                return BaseResponse<long>.Error(
                    Domain.Errors.ErrorCode.UPDATE_FAILED,
                    Domain.Errors.ErrorMessage.UPDATE_FAILED);
        }

        return BaseResponse<long>.Success(qr.QrId);
    }
}