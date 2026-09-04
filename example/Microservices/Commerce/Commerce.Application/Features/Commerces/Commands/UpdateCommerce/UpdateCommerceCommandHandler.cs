using Commerce.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Commands.UpdateCommerce;

public class UpdateCommerceCommandHandler(ICommerceRepository repository)
    : IRequestHandler<UpdateCommerceCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateCommerceCommand request, CancellationToken ct)
    {
        var existing = await repository.GetById(request.CommerceId, ct);
        if (existing is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND,
                Domain.Errors.ErrorMessage.COMMERCE_NOT_FOUND);

        if (!string.Equals(existing.Nit, request.Nit.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await repository.ExistsByNit(request.Nit.Trim(), ct);
            if (duplicate)
                return BaseResponse<long>.Error(
                    Domain.Errors.ErrorCode.COMMERCE_DUPLICATE,
                    Domain.Errors.ErrorMessage.COMMERCE_DUPLICATE);
        }

        var updated = await repository.Update(new()
        {
            CommerceId = request.CommerceId,
            Name       = request.Name.Trim(),
            Nit        = request.Nit.Trim(),
            IsActive   = existing.IsActive
        }, ct);

        if (updated <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.UPDATE_FAILED,
                Domain.Errors.ErrorMessage.UPDATE_FAILED);

        return BaseResponse<long>.Success(request.CommerceId);
    }
}
