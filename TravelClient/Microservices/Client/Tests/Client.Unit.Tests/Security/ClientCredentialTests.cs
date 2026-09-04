using Client.Domain.Security;
using FluentAssertions;
using Xunit;

namespace Client.Unit.Tests.Security;

public class ClientCredentialTests
{
    [Theory]
    [InlineData("Ana",          "ana",        "ana123")]
    [InlineData("  Ana  ",      "ana",        "ana123")]
    [InlineData("José",         "jose",       "jose123")]
    [InlineData("Ana María",    "anamaria",   "anamaria123")]
    [InlineData("Juan Carlos",  "juancarlos", "juancarlos123")]
    public void FromName_UsesTheNameAsUsernameAndTheNamePlus123AsPassword(
        string firstName, string expectedUsername, string expectedPassword)
    {
        var credential = ClientCredential.FromName(firstName);

        credential.Username.Should().Be(expectedUsername);
        credential.Password.Should().Be(expectedPassword);
    }
}
