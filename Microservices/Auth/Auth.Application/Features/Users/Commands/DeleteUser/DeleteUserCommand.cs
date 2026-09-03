using Core.Domain.Models;
using MediatR;

namespace Auth.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(long UserId) : IRequest<BaseResponse<long>>;
