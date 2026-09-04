using Country.Application.Features.Countries.Commands.CreateCountry;
using FluentAssertions;
using Xunit;

namespace Country.Unit.Tests.Features.Countries;

public class CreateCountryCommandValidatorTests
{
    private readonly CreateCountryCommandValidator _validator = new();

    [Theory]
    [InlineData("Perú", "PE")]
    [InlineData("Colombia", "co")]
    [InlineData("Estados Unidos", "US1")]
    public async Task Validate_WhenRequestIsValid_PassesValidation(string name, string code)
    {
        var result = await _validator.ValidateAsync(new CreateCountryCommand(name, code));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "PE")]
    [InlineData("Perú", "")]
    [InlineData("Perú", "   ")]
    [InlineData("Perú", "P E")]
    [InlineData("Perú", "P-E")]
    public async Task Validate_WhenRequestIsInvalid_FailsValidation(string name, string code)
    {
        var result = await _validator.ValidateAsync(new CreateCountryCommand(name, code));
        result.IsValid.Should().BeFalse();
    }
}
