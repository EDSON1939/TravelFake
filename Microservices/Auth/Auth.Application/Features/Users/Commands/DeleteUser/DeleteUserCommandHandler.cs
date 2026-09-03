using Auth.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Auth.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(IUserRepository repository)
    : IRequestHandler<DeleteUserCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        // El SP solo marca los que siguen vigentes: 0 filas significa que no
        // existe o que ya estaba dado de baja. En ambos casos no hay nada que hacer.
        var affected = await repository.Delete(request.UserId, ct);

        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.DELETE_FAILED,
                Domain.Errors.ErrorMessage.DELETE_FAILED);

        return BaseResponse<long>.Success(request.UserId);
    }
}
