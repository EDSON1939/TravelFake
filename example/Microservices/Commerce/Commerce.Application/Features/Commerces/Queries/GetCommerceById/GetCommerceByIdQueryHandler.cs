using Commerce.Application.Common;
using Commerce.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Queries.GetCommerceById;

public class GetCommerceByIdQueryHandler(ICommerceRepository repository)
    : IRequestHandler<GetCommerceByIdQuery, BaseResponse<CommerceResponse>>
{
    public async Task<BaseResponse<CommerceResponse>> Handle(GetCommerceByIdQuery request, CancellationToken ct)
    {
        var entity = await repository.GetById(request.CommerceId, ct);

        if (entity is null)
            return BaseResponse<CommerceResponse>.Error(
                Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND,
                Domain.Errors.ErrorMessage.COMMERCE_NOT_FOUND);

        return BaseResponse<CommerceResponse>.Success(new CommerceResponse(
            entity.CommerceId, entity.Name, entity.Nit,
            entity.CuentaId, entity.IsActive,
            entity.CreatedAt, entity.UpdatedAt));
    }
}
