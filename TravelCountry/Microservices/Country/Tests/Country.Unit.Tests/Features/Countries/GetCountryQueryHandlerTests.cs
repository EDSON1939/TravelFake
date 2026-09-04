using Country.Application.Features.Countries.Queries.GetCountry;
using Country.Domain.Entities;
using Country.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Country.Unit.Tests.Features.Countries;

public class GetCountryQueryHandlerTests
{
    private readonly ICountryRepository     _repository = Substitute.For<ICountryRepository>();
    private readonly GetCountryQueryHandler _handler;

    public GetCountryQueryHandlerTests() => _handler = new GetCountryQueryHandler(_repository);

    [Fact]
    public async Task Handle_WhenCountryExists_ReturnsSuccess()
    {
        // Arrange
        var entity = new CountryEntity
        {
            CountryId = 1, Name = "Perú", Code = "PE",
            IsActive = true, CreatedAt = DateTime.UtcNow
        };
        _repository.GetById(1L, default).Returns(entity);

        // Act
        var result = await _handler.Handle(new GetCountryQuery(1L), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("PE");
        result.Data.Name.Should().Be("Perú");
    }

    [Fact]
    public async Task Handle_WhenCountryNotFound_ReturnsError()
    {
        // Arrange
        _repository.GetById(99L, default).Returns((CountryEntity?)null);

        // Act
        var result = await _handler.Handle(new GetCountryQuery(99L), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        // Arrange
        _repository.GetById(Arg.Any<long>(), default).Returns((CountryEntity?)null);

        // Act
        await _handler.Handle(new GetCountryQuery(1L), default);

        // Assert
        await _repository.Received(1).GetById(1L, default);
    }
}
