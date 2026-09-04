using Client.Application.Features.Clients.Commands.UpdateClient;
using Client.Domain.Entities;
using Client.Domain.Repositories;
using Client.Domain.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Client.Unit.Tests.Features.Clients;

public class UpdateClientCommandHandlerTests
{
    private readonly IClientRepository          _repository     = Substitute.For<IClientRepository>();
    private readonly ICountryService            _countryService = Substitute.For<ICountryService>();
    private readonly UpdateClientCommandHandler _handler;

    public UpdateClientCommandHandlerTests()
    {
        // Por defecto TravelCountry responde con un país activo.
        _countryService.GetById(Arg.Any<long>(), default)
            .Returns(new CountryEntity { CountryId = 1L, Name = "Perú", Code = "PE", IsActive = true });

        _handler = new UpdateClientCommandHandler(_repository, _countryService);
    }

    private static UpdateClientCommand Command(long customerId = 1L, long countryId = 1L) =>
        new(customerId, "Ana", "Torres", "ana@mail.com", "999888777", countryId, true);

    [Fact]
    public async Task Handle_WhenUpdated_ReturnsSuccess()
    {
        // Arrange
        _repository.Update(Arg.Any<ClientEntity>(), default).Returns(1L);

        // Act
        var result = await _handler.Handle(Command(), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(1L);
    }

    [Fact]
    public async Task Handle_WhenCountryDoesNotExistInTravelCountry_ReturnsCountryNotFoundWithoutUpdating()
    {
        // Arrange — TravelCountry no conoce el país
        _countryService.GetById(999L, default).Returns((CountryEntity?)null);

        // Act
        var result = await _handler.Handle(Command(countryId: 999L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND);
        await _repository.DidNotReceive().Update(Arg.Any<ClientEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCountryIsInactive_ReturnsCountryInactiveWithoutUpdating()
    {
        // Arrange
        _countryService.GetById(5L, default)
            .Returns(new CountryEntity { CountryId = 5L, Name = "Chile", Code = "CL", IsActive = false });

        // Act
        var result = await _handler.Handle(Command(countryId: 5L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_INACTIVE);
        await _repository.DidNotReceive().Update(Arg.Any<ClientEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCountryDoesNotExist_ReturnsCountryNotFoundError()
    {
        // Arrange — travelfake.UPDATE_CLIENTE devuelve -1 por la FK_PAIS
        _repository.Update(Arg.Any<ClientEntity>(), default).Returns(-1L);

        // Act
        var result = await _handler.Handle(Command(countryId: 999L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ReturnsClientNotFoundError()
    {
        // Arrange — 0 filas afectadas
        _repository.Update(Arg.Any<ClientEntity>(), default).Returns(0L);

        // Act
        var result = await _handler.Handle(Command(customerId: 99L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.CLIENT_NOT_FOUND);
    }
}
