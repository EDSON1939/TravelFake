using Auth.Application.Features.Users.Commands.CreateUser;
using Auth.Domain.Entities;
using Auth.Domain.ExternalServices;
using Auth.Domain.Repositories;
using Auth.Domain.Security;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Auth.Unit.Tests.Features.Auth;

public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _repository     = Substitute.For<IUserRepository>();
    private readonly IClientService  _clientService  = Substitute.For<IClientService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
        => _handler = new CreateUserCommandHandler(_repository, _clientService, _passwordHasher);

    private static CreateUserCommand Command()
        => new("aquispe", "Secreta123", "70123456", "Ariel Quispe", UserRole.CLIENTE);

    private void ArrangeHappyPath()
    {
        _repository.GetByUsername("aquispe", default).Returns((UserEntity?)null);
        _repository.GetByClientId(1, default).Returns((UserEntity?)null);
        _clientService.GetByDocument("70123456", default)
            .Returns(new ClientInfo(1, "70123456", "Ariel Quispe", true));
        _passwordHasher.Hash("Secreta123").Returns("hash-calculado");
        _repository.Insert(Arg.Any<UserEntity>(), default).Returns(7L);
    }

    // ── Camino feliz ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenClientExists_CreatesTheUserLinkedToIt()
    {
        // Arrange
        ArrangeHappyPath();
        UserEntity? captured = null;
        _repository.Insert(Arg.Do<UserEntity>(x => captured = x), default).Returns(7L);

        // Act
        var result = await _handler.Handle(Command(), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(7L);
        captured!.ClientId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_StoresTheHashNeverThePlainPassword()
    {
        // Arrange
        ArrangeHappyPath();
        UserEntity? captured = null;
        _repository.Insert(Arg.Do<UserEntity>(x => captured = x), default).Returns(7L);

        // Act
        await _handler.Handle(Command(), default);

        // Assert
        captured!.PasswordHash.Should().Be("hash-calculado");
        captured.PasswordHash.Should().NotContain("Secreta123");
    }

    [Fact]
    public async Task Handle_NormalizesTheUsernameToLowercase()
    {
        // Arrange
        ArrangeHappyPath();
        UserEntity? captured = null;
        _repository.Insert(Arg.Do<UserEntity>(x => captured = x), default).Returns(7L);

        // Act
        await _handler.Handle(
            new CreateUserCommand("  AQuispe  ", "Secreta123", "70123456", " Ariel Quispe ", UserRole.CLIENTE),
            default);

        // Assert — el login busca en minúscula, así que guardar en minúscula
        // es lo que hace que "AQuispe" y "aquispe" sean el mismo usuario.
        captured!.Username.Should().Be("aquispe");
        captured.FullName.Should().Be("Ariel Quispe");
    }

    // ── Vínculo con el cliente ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenClientDoesNotExist_ReturnsClientNotFound()
    {
        // Arrange — guardar un ClientId sin verificarlo dejaría usuarios
        // apuntando a clientes inexistentes; coin.USUARIO no puede tener FK.
        _repository.GetByUsername("aquispe", default).Returns((UserEntity?)null);
        _clientService.GetByDocument("70123456", default).Returns((ClientInfo?)null);

        // Act
        var result = await _handler.Handle(Command(), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.CLIENT_NOT_FOUND);
        await _repository.DidNotReceive().Insert(Arg.Any<UserEntity>(), default);
    }

    [Fact]
    public async Task Handle_WhenClientIsInactive_ReturnsClientInactive()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default).Returns((UserEntity?)null);
        _clientService.GetByDocument("70123456", default)
            .Returns(new ClientInfo(1, "70123456", "Ariel Quispe", false));

        // Act
        var result = await _handler.Handle(Command(), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.CLIENT_INACTIVE);
        await _repository.DidNotReceive().Insert(Arg.Any<UserEntity>(), default);
    }

    [Fact]
    public async Task Handle_WhenClientAlreadyHasAUser_Rejects()
    {
        // Arrange — dos credenciales sobre el mismo cliente serían dos accesos
        // a la misma cuenta bancaria.
        ArrangeHappyPath();
        _repository.GetByClientId(1, default).Returns(new UserEntity { UserId = 3, ClientId = 1 });

        // Act
        var result = await _handler.Handle(Command(), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.CLIENT_ALREADY_HAS_USER);
        await _repository.DidNotReceive().Insert(Arg.Any<UserEntity>(), default);
    }

    [Fact]
    public async Task Handle_NormalizesTheDocumentBeforeLookup()
    {
        // Arrange
        ArrangeHappyPath();

        // Act
        await _handler.Handle(
            new CreateUserCommand("aquispe", "Secreta123", " 70123456 ", "Ariel Quispe", UserRole.CLIENTE),
            default);

        // Assert
        await _clientService.Received(1).GetByDocument("70123456", default);
    }

    // ── Operador interno sin cliente ─────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDocumentIsEmpty_CreatesAnInternalUserWithoutClient()
    {
        // Arrange
        _repository.GetByUsername("admin", default).Returns((UserEntity?)null);
        _passwordHasher.Hash("Secreta123").Returns("hash-calculado");
        UserEntity? captured = null;
        _repository.Insert(Arg.Do<UserEntity>(x => captured = x), default).Returns(7L);

        // Act
        var result = await _handler.Handle(
            new CreateUserCommand("admin", "Secreta123", "", "Operador", UserRole.ADMIN), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        captured!.ClientId.Should().BeNull();
        captured.Role.Should().Be(UserRole.ADMIN);
        await _clientService.DidNotReceive().GetByDocument(Arg.Any<string>(), default);
    }

    // ── Otros fallos ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUsernameIsTaken_ReturnsDuplicateError()
    {
        // Arrange
        _repository.GetByUsername("aquispe", default).Returns(new UserEntity { Username = "aquispe" });

        // Act
        var result = await _handler.Handle(Command(), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.USER_DUPLICATE);
        await _clientService.DidNotReceive().GetByDocument(Arg.Any<string>(), default);
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertFailedError()
    {
        // Arrange
        ArrangeHappyPath();
        _repository.Insert(Arg.Any<UserEntity>(), default).Returns(0L);

        // Act
        var result = await _handler.Handle(Command(), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
    }
}
