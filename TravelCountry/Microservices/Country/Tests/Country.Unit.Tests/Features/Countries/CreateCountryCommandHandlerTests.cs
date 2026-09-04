using Country.Application.Features.Countries.Commands.CreateCountry;
using Country.Domain.Entities;
using Country.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Country.Unit.Tests.Features.Countries;

public class CreateCountryCommandHandlerTests
{
    private readonly ICountryRepository          _repository = Substitute.For<ICountryRepository>();
    private readonly CreateCountryCommandHandler _handler;

    public CreateCountryCommandHandlerTests() => _handler = new CreateCountryCommandHandler(_repository);

    [Fact]
    public async Task Handle_WhenInsertSucceeds_ReturnsSuccessWithId()
    {
        // Arrange
        _repository.Insert(Arg.Any<CountryEntity>(), default).Returns(42L);

        // Act
        var result = await _handler.Handle(new CreateCountryCommand("Perú", "PE"), default);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(42L);
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ReturnsInsertError()
    {
        // Arrange
        _repository.Insert(Arg.Any<CountryEntity>(), default).Returns(0L);

        // Act
        var result = await _handler.Handle(new CreateCountryCommand("Colombia", "CO"), default);

        // Assert
        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.INSERT_FAILED);
    }

    [Fact]
    public async Task Handle_NormalizesCodeToUpperCaseAndTrimsName()
    {
        // Arrange
        CountryEntity? captured = null;
        _repository.Insert(Arg.Do<CountryEntity>(e => captured = e), default).Returns(1L);

        // Act
        await _handler.Handle(new CreateCountryCommand("  México  ", "mx"), default);

        // Assert
        captured.Should().NotBeNull();
        captured!.Code.Should().Be("MX");
        captured.Name.Should().Be("México");
    }
}
