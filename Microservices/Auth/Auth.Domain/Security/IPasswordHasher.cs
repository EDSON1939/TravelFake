namespace Auth.Domain.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>Compara en tiempo constante para no filtrar información por temporización.</summary>
    bool Verify(string password, string hash);
}
