using Client.Application.Features.Clients.Queries.GetClient;
using Client.Domain.Entities;
using Client.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Client.Unit.Tests.Features.Clients;

public class GetClientQueryHandlerTests
{
    private readonly IClientRepository     _repository = Substitute.For<IClientRepository>();
    private readonly GetClientQueryHandler _handler;

    public GetClientQueryHandlerTests() => _handler = new GetClientQueryHandler(_repository);

    [Fact]
    public async Task Handle_WhenClientExists_ReturnsSuccessWithCountryData()
    {
        // Arrange
        var entity = new ClientEntity
        {
            CustomerId = 1, FirstName = "Ana", LastName = "Torres",
            Email = "ana@mail.com", Phone = "999888777",
            CountryId = 5, CountryName = "Perú", CountryCode = "PE",
            IsActive = true, CreatedAt = DateTime.UtcNow
        };
        _repository.GetById(1L, default).Returns(entity);

        // Act
        var result = await _handler.Handle(new GetClientQuery(1L), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("ana@mail.com");
        result.Data.CountryName.Should().Be("Perú");
        result.Data.CountryCode.Should().Be("PE");
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ReturnsError()
    {
        // Arrange
        _repository.GetById(99L, default).Returns((ClientEntity?)null);

        // Act
        var result = await _handler.Handle(new GetClientQuery(99L), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.CLIENT_NOT_FOUND);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        // Arrange
        _repository.GetById(Arg.Any<long>(), default).Returns((ClientEntity?)null);

        // Act
        await _handler.Handle(new GetClientQuery(1L), default);

        // Assert
        await _repository.Received(1).GetById(1L, default);
    }
}
