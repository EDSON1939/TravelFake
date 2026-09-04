using Client.Application.Features.Clients.Commands.CreateClient;
using FluentAssertions;
using Xunit;

namespace Client.Unit.Tests.Features.Clients;

public class CreateClientCommandValidatorTests
{
    private readonly CreateClientCommandValidator _validator = new();

    [Theory]
    [InlineData("Ana", "Torres", "ana@mail.com", "999888777", 1)]
    [InlineData("Luis", "Gómez", "luis.gomez@empresa.pe", "+51 (1) 999-888", 5)]
    public async Task Validate_WhenRequestIsValid_PassesValidation(
        string firstName, string lastName, string email, string phone, long countryId)
    {
        var result = await _validator.ValidateAsync(
            new CreateClientCommand(firstName, lastName, email, phone, countryId));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Torres", "ana@mail.com", "999888777", 1)]        // nombre vacío
    [InlineData("Ana", "", "ana@mail.com", "999888777", 1)]           // apellido vacío
    [InlineData("Ana", "Torres", "correo-invalido", "999888777", 1)]  // email sin formato
    [InlineData("Ana", "Torres", "ana@mail.com", "abc", 1)]           // teléfono con letras
    [InlineData("Ana", "Torres", "ana@mail.com", "999888777", 0)]     // país no informado
    public async Task Validate_WhenRequestIsInvalid_FailsValidation(
        string firstName, string lastName, string email, string phone, long countryId)
    {
        var result = await _validator.ValidateAsync(
            new CreateClientCommand(firstName, lastName, email, phone, countryId));
        result.IsValid.Should().BeFalse();
    }
}
