using Core.Domain.Models;
using MediatR;
using Qr.Application.Common;
using Qr.Domain.Entities;
using Qr.Domain.Interfaces;
using Qr.Domain.Repositories;

namespace Qr.Application.Features.Qrs.Commands.CreateQr;

public class CreateQrCommandHandler(
    IQrRepository repository,
    ICommerceService commerceService)
    : IRequestHandler<CreateQrCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateQrCommand request, CancellationToken ct)
    {
        // 1. Validar comercio
        var commerce = await commerceService.ExistsAsync(request.ComercioId, ct);
        if (!commerce.IsSuccess())
            return BaseResponse<long>.Error(commerce.StatusCode, commerce.Message!);

        // 2. Generar código único y guardar
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = QrCodeGenerator.Generate();

            var existing = await repository.GetByCode(code, ct);
            if (existing is not null) continue;

            var id = await repository.Insert(new QrEntity
            {
                Codigo          = code,
                ComercioId      = request.ComercioId,
                Monto           = request.Monto,
                Tipo            = request.Tipo.ToUpper(),
                FechaExpiracion = request.FechaExpiracion,
                Estado          = QrEstado.ACTIVE,
                Activo          = true
            }, ct);

            if (id <= 0)
                return BaseResponse<long>.Error(
                    Domain.Errors.ErrorCode.INSERT_FAILED,
                    Domain.Errors.ErrorMessage.INSERT_FAILED);

            return BaseResponse<long>.Success(id);
        }

        return BaseResponse<long>.Error(
            Domain.Errors.ErrorCode.QR_DUPLICATE,
            Domain.Errors.ErrorMessage.QR_DUPLICATE);
    }
}