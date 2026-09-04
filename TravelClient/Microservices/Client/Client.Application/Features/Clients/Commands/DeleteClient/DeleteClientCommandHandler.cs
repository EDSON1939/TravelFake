using Client.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.DeleteClient;

public class DeleteClientCommandHandler(IClientRepository repository)
    : IRequestHandler<DeleteClientCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(DeleteClientCommand request, CancellationToken ct)
    {
        var affected = await repository.Delete(request.CustomerId, ct);

        // travelfake.DELETE_CLIENTE devuelve 0 cuando el CLIENTES_ID_IT no existe
        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CLIENT_NOT_FOUND,
                Domain.Errors.ErrorMessage.CLIENT_NOT_FOUND);

        return BaseResponse<long>.Success(request.CustomerId);
    }
}
