using Country.Application.Features.Countries.Commands.DeleteCountry;
using Country.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Country.Unit.Tests.Features.Countries;

public class DeleteCountryCommandHandlerTests
{
    private readonly ICountryRepository          _repository = Substitute.For<ICountryRepository>();
    private readonly DeleteCountryCommandHandler _handler;

    public DeleteCountryCommandHandlerTests() => _handler = new DeleteCountryCommandHandler(_repository);

    [Fact]
    public async Task Handle_WhenDeleted_ReturnsSuccess()
    {
        // Arrange — dbo.DELETE_PAIS devuelve 1 fila afectada
        _repository.Delete(5L, default).Returns(1L);

        // Act
        var result = await _handler.Handle(new DeleteCountryCommand(5L), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(5L);
    }

    [Fact]
    public async Task Handle_WhenCountryHasCustomers_ReturnsInUseError()
    {
        // Arrange — dbo.DELETE_PAIS devuelve -1 por la FK_PAIS
        _repository.Delete(5L, default).Returns(-1L);

        // Act
        var result = await _handler.Handle(new DeleteCountryCommand(5L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_IN_USE);
    }

    [Fact]
    public async Task Handle_WhenCountryNotFound_ReturnsNotFoundError()
    {
        // Arrange — dbo.DELETE_PAIS devuelve 0 filas afectadas
        _repository.Delete(99L, default).Returns(0L);

        // Act
        var result = await _handler.Handle(new DeleteCountryCommand(99L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND);
    }
}
