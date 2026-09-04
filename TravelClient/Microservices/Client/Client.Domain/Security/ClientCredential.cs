using System.Globalization;
using System.Text;

namespace Client.Domain.Security;

/// <summary>
/// Credencial por defecto con la que se da de alta al cliente en el microservicio
/// Auth cuando se lo registra: el usuario es su nombre y la contraseña ese mismo
/// nombre seguido de "123" (por ejemplo, "Ana" → usuario "ana", clave "ana123").
/// </summary>
public sealed record ClientCredential(string Username, string Password)
{
    /// <summary>Sufijo fijo que se le añade al nombre para armar la contraseña.</summary>
    public const string PasswordSuffix = "123";

    public static ClientCredential FromName(string firstName)
    {
        var username = Normalize(firstName);
        return new ClientCredential(username, username + PasswordSuffix);
    }

    /// <summary>
    /// Auth solo acepta usuarios que casen con ^[a-zA-Z0-9._-]+$, así que el nombre
    /// se pasa a minúsculas, se le quitan los acentos ("José" → "jose") y se
    /// descarta todo carácter que no entre en esa lista (espacios incluidos).
    /// </summary>
    private static string Normalize(string firstName)
    {
        var decomposed = firstName.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            // Las tildes quedan como marcas sueltas al descomponer: se descartan.
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-')
                builder.Append(character);
        }

        return builder.ToString();
    }
}
