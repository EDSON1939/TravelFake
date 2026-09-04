using Client.Application.Features.Clients.Queries.GetClients;
using Client.Domain.Entities;
using Client.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Client.Unit.Tests.Features.Clients;

public class GetClientsQueryHandlerTests
{
    private readonly IClientRepository      _repository = Substitute.For<IClientRepository>();
    private readonly GetClientsQueryHandler _handler;

    public GetClientsQueryHandlerTests() => _handler = new GetClientsQueryHandler(_repository);

    [Fact]
    public async Task Handle_ReturnsMappedList()
    {
        // Arrange
        _repository.GetAll(true, null, default).Returns(new List<ClientEntity>
        {
            new() { CustomerId = 1, FirstName = "Ana",  LastName = "Torres", CountryCode = "PE" },
            new() { CustomerId = 2, FirstName = "Luis", LastName = "Gómez",  CountryCode = "CO" },
        });

        // Act
        var result = await _handler.Handle(new GetClientsQuery(), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().HaveCount(2);
        result.Data![0].FirstName.Should().Be("Ana");
    }

    [Fact]
    public async Task Handle_ForwardsCountryFilterToRepository()
    {
        // Arrange
        _repository.GetAll(Arg.Any<bool>(), Arg.Any<long?>(), default).Returns([]);

        // Act
        await _handler.Handle(new GetClientsQuery(OnlyActive: false, CountryId: 7L), default);

        // Assert
        await _repository.Received(1).GetAll(false, 7L, default);
    }

    [Fact]
    public async Task Handle_WhenNoClients_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetAll(Arg.Any<bool>(), Arg.Any<long?>(), default).Returns([]);

        // Act
        var result = await _handler.Handle(new GetClientsQuery(), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().BeEmpty();
    }
}
