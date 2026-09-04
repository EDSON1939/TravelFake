using Client.Application.Features.Clients.Commands.CreateClient;
using Client.Domain.Entities;
using Client.Domain.Repositories;
using Client.Domain.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Client.Unit.Tests.Features.Clients;

public class CreateClientCommandHandlerTests
{
    private readonly IClientRepository          _repository     = Substitute.For<IClientRepository>();
    private readonly ICountryService            _countryService = Substitute.For<ICountryService>();
    private readonly CreateClientCommandHandler _handler;

    public CreateClientCommandHandlerTests()
    {
        // Por defecto TravelCountry responde con un país activo.
        _countryService.GetById(Arg.Any<long>(), default)
            .Returns(new CountryEntity { CountryId = 1L, Name = "Perú", Code = "PE", IsActive = true });

        _handler = new CreateClientCommandHandler(_repository, _countryService);
    }

    [Fact]
    public async Task Handle_WhenInsertSucceeds_ReturnsSuccessWithId()
    {
        // Arrange
        _repository.Insert(Arg.Any<ClientEntity>(), default).Returns(42L);

        // Act
        var result = await _handler.Handle(
            new CreateClientCommand("Ana", "Torres", "ana@mail.com", "999888777", 1L), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(42L);
    }

    [Fact]
    public async Task Handle_WhenCountryDoesNotExistInTravelCountry_ReturnsCountryNotFoundWithoutInserting()
    {
        // Arrange — TravelCountry no conoce el país
        _countryService.GetById(999L, default).Returns((CountryEntity?)null);

        // Act
        var result = await _handler.Handle(
            new CreateClientCommand("Ana", "Torres", "ana@mail.com", "999888777", 999L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND);
        await _repository.DidNotReceive().Insert(Arg.Any<ClientEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCountryIsInactive_ReturnsCountryInactiveWithoutInserting()
    {
        // Arrange
        _countryService.GetById(5L, default)
            .Returns(new CountryEntity { CountryId = 5L, Name = "Chile", Code = "CL", IsActive = false });

        // Act
        var result = await _handler.Handle(
            new CreateClientCommand("Ana", "Torres", "ana@mail.com", "999888777", 5L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_INACTIVE);
        await _repository.DidNotReceive().Insert(Arg.Any<ClientEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCountryDoesNotExist_ReturnsCountryNotFoundError()
    {
        // Arrange — travelfake.INSERT_CLIENTE devuelve -1 por la FK_PAIS
        _repository.Insert(Arg.Any<ClientEntity>(), default).Returns(-1L);

        // Act
        var result = await _handler.Handle(
            new CreateClientCommand("Ana", "Torres", "ana@mail.com", "999888777", 999L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertError()
    {
        // Arrange
        _repository.Insert(Arg.Any<ClientEntity>(), default).Returns(0L);

        // Act
        var result = await _handler.Handle(
            new CreateClientCommand("Ana", "Torres", "ana@mail.com", "999888777", 1L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
    }

    [Fact]
    public async Task Handle_NormalizesEmailToLowerCaseAndTrimsFields()
    {
        // Arrange
        ClientEntity? captured = null;
        _repository.Insert(Arg.Do<ClientEntity>(e => captured = e), default).Returns(1L);

        // Act
        await _handler.Handle(
            new CreateClientCommand("  Ana  ", "  Torres  ", "  ANA@Mail.COM  ", " 999888777 ", 1L), default);

        // Assert
        captured.Should().NotBeNull();
        captured!.Email.Should().Be("ana@mail.com");
        captured.FirstName.Should().Be("Ana");
        captured.LastName.Should().Be("Torres");
        captured.Phone.Should().Be("999888777");
    }
}
