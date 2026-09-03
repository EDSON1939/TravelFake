using Auth.Api.Grpc;
using Auth.Application.Features.Users.Commands.CreateUser;
using Auth.Application.Features.Users.Commands.DeleteUser;
using Auth.Application.Features.Users.Commands.Login;
using AutoMapper;
using Grpc.Core;
using MediatR;

namespace Auth.Api.Services;

public class AuthService(ISender sender, IMapper mapper) : Auth.Api.Grpc.Auth.AuthBase
{
    public override async Task<LoginBaseResponsePb> Login(
        LoginRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new LoginCommand(request.Username, request.Password),
            context.CancellationToken);

        return mapper.Map<LoginBaseResponsePb>(result);
    }

    public override async Task<UserMutationBaseResponsePb> CreateUser(
        CreateUserRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateUserCommand(
                request.Username, request.Password, request.Document, request.FullName, request.Role),
            context.CancellationToken);

        return mapper.Map<UserMutationBaseResponsePb>(result);
    }

    public override async Task<UserMutationBaseResponsePb> DeleteUser(
        DeleteUserRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new DeleteUserCommand(request.UserId),
            context.CancellationToken);

        return mapper.Map<UserMutationBaseResponsePb>(result);
    }
}
