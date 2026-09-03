using Auth.Application.Features.Users.Commands.Login;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Domain.Security;
using Core.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Auth.Unit.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository    _repository     = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher    _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:MaxFailedAttempts"] = "5",
                ["Jwt:LockMinutes"]       = "15"
            })
            .Build();

        _handler = new LoginCommandHandler(_repository, _passwordHasher, _tokenGenerator, configuration);
    }

    private static UserEntity User(bool isActive = true, DateTime? lockedUntil = null)
        => new()
        {
            UserId       = 7,
            Username     = "aquispe",
            PasswordHash = "hash-guardado",
            ClientId     = 1,
            FullName     = "Ariel Quispe",
            Role         = UserRole.CLIENTE,
            IsActive     = isActive,
            LockedUntil  = lockedUntil
        };

    private void ArrangeHappyPath()
    {
        _repository.GetByUsername("aquispe", default).Returns(User());
        _passwordHasher.Verify("Secreta123", "hash-guardado").Returns(true);
        _tokenGenerator.Generate(Arg.Any<string>(), Arg.Any<IDictionary<string, string>>(), Arg.Any<IEnumerable<string>>())
            .Returns(new JwtToken("token-firmado", new DateTime(2026, 1, 1, 0, 5, 0, DateTimeKind.Utc), 300));
    }

    // ── Camino feliz ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsToken()
    {
        // Arrange
        ArrangeHappyPath();

        // Act
        var result = await _handler.Handle(new LoginCommand("aquispe", "Secreta123"), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.AccessToken.Should().Be("token-firmado");
        result.Data.UserId.Should().Be(7);
        result.Data.ClientId.Should().Be(1);
        result.Data.Role.Should().Be(UserRole.CLIENTE);
    }

    [Fact]
    public async Task Handle_PutsRoleAndClientIdInTheTokenClaims()
    {
        // Arrange
        ArrangeHappyPath();
        IDictionary<string, string>? claims = null;
        _tokenGenerator.Generate(
                Arg.Any<string>(),
                Arg.Do<IDictionary<string, string>>(x => claims = x),
                Arg.Any<IEnumerable<string>>())
            .Returns(new JwtToken("token-firmado", DateTime.UtcNow, 300));

        // Act
        await _handler.Handle(new LoginCommand("aquispe", "Secreta123"), default);

        // Assert
        claims!["role"].Should().Be(UserRole.CLIENTE);
        claims["client_id"].Should().Be("1");
        claims["username"].Should().Be("aquispe");
    }

    [Fact]
    public async Task Handle_OnSuccess_ResetsTheFailedAttemptCounter()
    {
        // Arrange
        ArrangeHappyPath();

        // Act
        await _handler.Handle(new LoginCommand("aquispe", "Secreta123"), default);

        // Assert
        await _repository.Received(1).RegisterLoginAttempt(7, true, 5, 15, default);
    }

    [Fact]
    public async Task Handle_NormalizesTheUsernameButNotThePassword()
    {
        // Arrange
        ArrangeHappyPath();

        // Act
        await _handler.Handle(new LoginCommand("  AQuispe  ", "Secreta123"), default);

        // Assert — recortar la contraseña cambiaría el secreto del usuario
        await _repository.Received(1).GetByUsername("aquispe", default);
        _passwordHasher.Received(1).Verify("Secreta123", "hash-guardado");
    }

    // ── Credenciales inválidas ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsGenericError()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default).Returns((UserEntity?)null);

        // Act
        var result = await _handler.Handle(new LoginCommand("aquispe", "Secreta123"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INVALID_CREDENTIALS);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ReturnsTheSameErrorAsUnknownUser()
    {
        // Arrange — mismo código en ambos casos: si difirieran, un atacante
        // podría enumerar qué usuarios existen.
        _repository.GetByUsername("aquispe", default).Returns(User());
        _passwordHasher.Verify("mala", "hash-guardado").Returns(false);

        // Act
        var result = await _handler.Handle(new LoginCommand("aquispe", "mala"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INVALID_CREDENTIALS);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_CountsTheFailedAttempt()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default).Returns(User());
        _passwordHasher.Verify("mala", "hash-guardado").Returns(false);

        // Act
        await _handler.Handle(new LoginCommand("aquispe", "mala"), default);

        // Assert — sin contador no hay freno a la fuerza bruta
        await _repository.Received(1).RegisterLoginAttempt(7, false, 5, 15, default);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_DoesNotIssueAToken()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default).Returns(User());
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        await _handler.Handle(new LoginCommand("aquispe", "mala"), default);

        // Assert
        _tokenGenerator.DidNotReceive().Generate(
            Arg.Any<string>(), Arg.Any<IDictionary<string, string>>(), Arg.Any<IEnumerable<string>>());
    }

    // ── Bloqueo e inactividad ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUserIsLocked_RejectsWithoutCheckingThePassword()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default)
            .Returns(User(lockedUntil: DateTime.UtcNow.AddMinutes(10)));

        // Act
        var result = await _handler.Handle(new LoginCommand("aquispe", "Secreta123"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.USER_LOCKED);
        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenTheLockHasExpired_AllowsTheLogin()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default)
            .Returns(User(lockedUntil: DateTime.UtcNow.AddMinutes(-1)));
        _passwordHasher.Verify("Secreta123", "hash-guardado").Returns(true);
        _tokenGenerator.Generate(Arg.Any<string>(), Arg.Any<IDictionary<string, string>>(), Arg.Any<IEnumerable<string>>())
            .Returns(new JwtToken("token-firmado", DateTime.UtcNow, 300));

        // Act
        var result = await _handler.Handle(new LoginCommand("aquispe", "Secreta123"), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ReturnsInactiveError()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default).Returns(User(isActive: false));

        // Act
        var result = await _handler.Handle(new LoginCommand("aquispe", "Secreta123"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.USER_INACTIVE);
    }
}
