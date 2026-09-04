using Commerce.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Queries.GetCommerceList;

public record GetCommerceListQuery(
    string? Name = null,
    bool?   OnlyActive = null,
    int     PageNumber = 1,
    int     PageSize = 10)
    : IRequest<BaseResponse<PagedResult<CommerceResponse>>>;
