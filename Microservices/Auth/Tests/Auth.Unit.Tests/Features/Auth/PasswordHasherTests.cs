using Auth.Infrastructure.Security;
using FluentAssertions;
using Xunit;

namespace Auth.Unit.Tests.Features.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_AcceptsTheCorrectPassword()
    {
        var hash = _hasher.Hash("Secreta123");

        _hasher.Verify("Secreta123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_RejectsTheWrongPassword()
    {
        var hash = _hasher.Hash("Secreta123");

        _hasher.Verify("Secreta124", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        var hash = _hasher.Hash("Secreta123");

        _hasher.Verify("secreta123", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_ProducesADifferentValueEachTime()
    {
        // Salt distinto por usuario: dos personas con la misma contraseña no
        // comparten hash, así una tabla rainbow no sirve de nada.
        _hasher.Hash("Secreta123").Should().NotBe(_hasher.Hash("Secreta123"));
    }

    [Fact]
    public void Hash_StoresIterationsSaltAndHash()
    {
        var parts = _hasher.Hash("Secreta123").Split('.');

        parts.Should().HaveCount(3);
        int.Parse(parts[0]).Should().BeGreaterThanOrEqualTo(100_000);
    }

    [Fact]
    public void Hash_NeverContainsThePlainPassword()
    {
        _hasher.Hash("Secreta123").Should().NotContain("Secreta123");
    }

    [Theory]
    [InlineData("")]
    [InlineData("basura")]
    [InlineData("100000.no-es-base64.tampoco")]
    [InlineData("abc.c2FsdA==.aGFzaA==")]
    public void Verify_ReturnsFalseOnMalformedHashInsteadOfThrowing(string hash)
    {
        // Un hash corrupto en base de datos no debe tumbar el login del servicio.
        _hasher.Verify("Secreta123", hash).Should().BeFalse();
    }
}
