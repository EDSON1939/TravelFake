using Commerce.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Commands.UpdateStatus;

public class UpdateCommerceStatusCommandHandler(ICommerceRepository repository)
    : IRequestHandler<UpdateCommerceStatusCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateCommerceStatusCommand request, CancellationToken ct)
    {
        var existing = await repository.GetById(request.CommerceId, ct);
        if (existing is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND,
                Domain.Errors.ErrorMessage.COMMERCE_NOT_FOUND);

        var affected = await repository.UpdateStatus(request.CommerceId, request.IsActive, ct);

        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.UPDATE_FAILED,
                Domain.Errors.ErrorMessage.UPDATE_FAILED);

        return BaseResponse<long>.Success(request.CommerceId);
    }
}
