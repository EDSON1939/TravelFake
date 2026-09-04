using Client.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.UpdateClientStatus;

public class UpdateClientStatusCommandHandler(IClientRepository repository)
    : IRequestHandler<UpdateClientStatusCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateClientStatusCommand request, CancellationToken ct)
    {
        var affected = await repository.UpdateStatus(request.CustomerId, request.IsActive, ct);

        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CLIENT_NOT_FOUND,
                Domain.Errors.ErrorMessage.CLIENT_NOT_FOUND);

        return BaseResponse<long>.Success(request.CustomerId);
    }
}
