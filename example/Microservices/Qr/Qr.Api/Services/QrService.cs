using AutoMapper;
using Grpc.Core;
using MediatR;
using Qr.Api.Grpc;
using Qr.Application.Features.Qrs.Commands.ConsumeQr;
using Qr.Application.Features.Qrs.Commands.CreateQr;
using Qr.Application.Features.Qrs.Queries.GetQrByCode;
using Qr.Application.Features.Qrs.Queries.GetQrById;
using System.Globalization;

namespace Qr.Api.Services;

public class QrService(ISender sender, IMapper mapper) : Qr.Api.Grpc.Qr.QrBase
{
    public override async Task<QrMutationBaseResponsePb> CreateQr(
        CreateQrRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateQrCommand(
                request.ComercioId,
                decimal.Parse(request.Monto, CultureInfo.InvariantCulture),
                request.Tipo,
                DateTime.Parse(request.FechaExpiracion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)),
            context.CancellationToken);

        return mapper.Map<QrMutationBaseResponsePb>(result);
    }

    public override async Task<GetQrBaseResponsePb> GetQrById(
        GetQrByIdRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetQrByIdQuery(request.Id),
            context.CancellationToken);

        return mapper.Map<GetQrBaseResponsePb>(result);
    }

    public override async Task<GetQrBaseResponsePb> GetQrByCode(
        GetQrByCodeRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetQrByCodeQuery(request.Code),
            context.CancellationToken);

        return mapper.Map<GetQrBaseResponsePb>(result);
    }

    public override async Task<QrMutationBaseResponsePb> ConsumeQr(
        ConsumeQrRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new ConsumeQrCommand(request.Code),
            context.CancellationToken);

        return mapper.Map<QrMutationBaseResponsePb>(result);
    }
}