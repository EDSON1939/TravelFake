using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Commands.UpdateCommerce;

public record UpdateCommerceCommand(long CommerceId, string Name, string Nit)
    : IRequest<BaseResponse<long>>;
