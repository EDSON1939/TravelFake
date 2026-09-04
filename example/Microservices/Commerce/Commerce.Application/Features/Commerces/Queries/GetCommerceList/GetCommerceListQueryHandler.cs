using Commerce.Application.Common;
using Commerce.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Queries.GetCommerceList;

public class GetCommerceListQueryHandler(ICommerceRepository repository)
    : IRequestHandler<GetCommerceListQuery, BaseResponse<PagedResult<CommerceResponse>>>
{
    public async Task<BaseResponse<PagedResult<CommerceResponse>>> Handle(
        GetCommerceListQuery request, CancellationToken ct)
    {
        var (items, total) = await repository.GetAll(
            request.Name, request.OnlyActive, request.PageNumber, request.PageSize, ct);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        var result = new PagedResult<CommerceResponse>(
            items.Select(e => new CommerceResponse(
                e.CommerceId, e.Name, e.Nit,
                e.CuentaId, e.IsActive,
                e.CreatedAt, e.UpdatedAt)),
            request.PageNumber, request.PageSize, total, totalPages);

        return BaseResponse<PagedResult<CommerceResponse>>.Success(result);
    }
}
