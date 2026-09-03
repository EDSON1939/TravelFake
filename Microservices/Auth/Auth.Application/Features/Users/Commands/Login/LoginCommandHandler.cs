using Auth.Application.Common;
using Auth.Domain.Repositories;
using Auth.Domain.Security;
using Core.Domain.Models;
using Core.Infrastructure.Security;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Features.Users.Commands.Login;

/// <summary>
/// Verifica credenciales y emite el JWT que exigen los demás microservicios.
/// Este es el único punto del sistema que emite tokens.
/// </summary>
public class LoginCommandHandler(
    IUserRepository repository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IConfiguration configuration)
    : IRequestHandler<LoginCommand, BaseResponse<LoginResponse>>
{
    private const int DefaultMaxAttempts = 5;
    private const int DefaultLockMinutes = 15;

    public async Task<BaseResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var maxAttempts = configuration.GetValue<int?>("Jwt:MaxFailedAttempts") ?? DefaultMaxAttempts;
        var lockMinutes = configuration.GetValue<int?>("Jwt:LockMinutes")       ?? DefaultLockMinutes;

        var user = await repository.GetByUsername(request.Username.Trim().ToLowerInvariant(), ct);

        if (user is null)
            return InvalidCredentials();

        if (user.IsLocked(DateTime.UtcNow))
            return BaseResponse<LoginResponse>.Error(
                Domain.Errors.ErrorCode.USER_LOCKED,
                Domain.Errors.ErrorMessage.USER_LOCKED);

        if (!user.IsActive)
            return BaseResponse<LoginResponse>.Error(
                Domain.Errors.ErrorCode.USER_INACTIVE,
                Domain.Errors.ErrorMessage.USER_INACTIVE);

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // El contador es lo que frena la fuerza bruta: sin él, se puede
            // probar contraseñas indefinidamente.
            await repository.RegisterLoginAttempt(user.UserId, false, maxAttempts, lockMinutes, ct);
            return InvalidCredentials();
        }

        await repository.RegisterLoginAttempt(user.UserId, true, maxAttempts, lockMinutes, ct);

        var claims = new Dictionary<string, string>
        {
            ["username"] = user.Username,
            ["role"]     = user.Role
        };

        if (user.ClientId.HasValue)
            claims["client_id"] = user.ClientId.Value.ToString();

        var token = tokenGenerator.Generate(
            subject: user.UserId.ToString(),
            claims: claims,
            permissions: [user.Role]);

        return BaseResponse<LoginResponse>.Success(new LoginResponse(
            token.AccessToken,
            token.ExpiresAt,
            token.ExpiresInSeconds,
            user.UserId,
            user.ClientId,
            user.Username,
            user.FullName,
            user.Role));
    }

    private static BaseResponse<LoginResponse> InvalidCredentials()
        => BaseResponse<LoginResponse>.Error(
            Domain.Errors.ErrorCode.INVALID_CREDENTIALS,
            Domain.Errors.ErrorMessage.INVALID_CREDENTIALS);
}
