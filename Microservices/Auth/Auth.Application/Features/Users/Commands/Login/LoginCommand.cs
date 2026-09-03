using Auth.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Auth.Application.Features.Users.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<BaseResponse<LoginResponse>>;
