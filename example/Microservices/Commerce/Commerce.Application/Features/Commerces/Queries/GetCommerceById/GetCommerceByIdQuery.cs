using Commerce.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Queries.GetCommerceById;

public record GetCommerceByIdQuery(long CommerceId)
    : IRequest<BaseResponse<CommerceResponse>>;
