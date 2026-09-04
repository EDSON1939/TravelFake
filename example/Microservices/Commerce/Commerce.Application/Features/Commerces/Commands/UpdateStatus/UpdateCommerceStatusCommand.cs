using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Commands.UpdateStatus;

public record UpdateCommerceStatusCommand(long CommerceId, bool IsActive)
    : IRequest<BaseResponse<long>>;
