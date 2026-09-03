using Auth.Domain.Security;
using System.Security.Cryptography;

namespace Auth.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 con salt por usuario. Formato almacenado:
/// "iteraciones.saltBase64.hashBase64" — el salt y el costo viajan con el hash,
/// así se puede subir el número de iteraciones sin invalidar los hashes viejos.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int    Iterations = 100_000;
    private const int    SaltSize   = 16;
    private const int    HashSize   = 32;
    private const char   Separator  = '.';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return string.Join(Separator, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split(Separator);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        byte[] salt, expected;
        try
        {
            salt     = Convert.FromBase64String(parts[1]);
            expected = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

        // Comparación en tiempo fijo: un == normal cortaría en el primer byte
        // distinto y filtraría información por el tiempo de respuesta.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
