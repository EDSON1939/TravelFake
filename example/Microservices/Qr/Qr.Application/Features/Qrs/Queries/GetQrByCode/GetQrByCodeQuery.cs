using Core.Domain.Models;
using MediatR;
using Qr.Application.Common;

namespace Qr.Application.Features.Qrs.Queries.GetQrByCode;

public record GetQrByCodeQuery(string Code) : IRequest<BaseResponse<QrResponse>>;