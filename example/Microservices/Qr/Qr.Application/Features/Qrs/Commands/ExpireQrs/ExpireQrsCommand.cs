using Core.Domain.Models;
using MediatR;

namespace Qr.Application.Features.Qrs.Commands.ExpireQrs;

public record ExpireQrsCommand : IRequest<BaseResponse<long>>;