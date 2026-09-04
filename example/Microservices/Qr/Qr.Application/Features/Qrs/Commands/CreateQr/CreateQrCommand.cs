using Core.Domain.Models;
using MediatR;

namespace Qr.Application.Features.Qrs.Commands.CreateQr;

public record CreateQrCommand(
    long ComercioId,
    decimal Monto,
    string Tipo,
    DateTime FechaExpiracion)
    : IRequest<BaseResponse<long>>;