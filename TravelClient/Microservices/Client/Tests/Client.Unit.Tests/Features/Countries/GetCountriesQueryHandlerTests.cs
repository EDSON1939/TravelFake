using Client.Application.Features.Countries.Queries.GetCountries;
using Client.Domain.Entities;
using Client.Domain.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Client.Unit.Tests.Features.Countries;

public class GetCountriesQueryHandlerTests
{
    private readonly ICountryService          _countryService = Substitute.For<ICountryService>();
    private readonly GetCountriesQueryHandler _handler;

    public GetCountriesQueryHandlerTests() => _handler = new GetCountriesQueryHandler(_countryService);

    [Fact]
    public async Task Handle_ReturnsCountriesFromTravelCountry()
    {
        // Arrange
        _countryService.GetAll(true, default).Returns(new List<CountryEntity>
        {
            new() { CountryId = 1L, Name = "Perú",  Code = "PE", IsActive = true },
            new() { CountryId = 2L, Name = "Chile", Code = "CL", IsActive = true }
        });

        // Act
        var result = await _handler.Handle(new GetCountriesQuery(true), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().HaveCount(2);
        result.Data![0].Code.Should().Be("PE");
    }

    [Fact]
    public async Task Handle_WhenTravelCountryReturnsNothing_ReturnsEmptyList()
    {
        // Arrange
        _countryService.GetAll(false, default).Returns([]);

        // Act
        var result = await _handler.Handle(new GetCountriesQuery(false), default);

        // Assert
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().BeEmpty();
    }
}
