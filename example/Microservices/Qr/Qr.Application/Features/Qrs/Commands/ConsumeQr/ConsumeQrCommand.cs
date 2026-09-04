using Core.Domain.Models;
using MediatR;

namespace Qr.Application.Features.Qrs.Commands.ConsumeQr;

public record ConsumeQrCommand(string Code) : IRequest<BaseResponse<long>>;