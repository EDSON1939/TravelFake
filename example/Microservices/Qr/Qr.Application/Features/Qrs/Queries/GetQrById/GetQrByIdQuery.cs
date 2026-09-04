using Core.Domain.Models;
using MediatR;
using Qr.Application.Common;

namespace Qr.Application.Features.Qrs.Queries.GetQrById;

public record GetQrByIdQuery(long QrId) : IRequest<BaseResponse<QrResponse>>;