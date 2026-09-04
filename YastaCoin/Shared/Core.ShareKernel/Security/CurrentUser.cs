using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Core.ShareKernel.Security;

/// <summary>
/// Lee los claims del JWT que llega en la cabecera Authorization.
///
/// LEE el token, no lo VALIDA: la firma y la vigencia las verifica el Gateway,
/// unico punto de entrada, y repetirlo en cada microservicio obligaria a
/// repartir el secreto por todo el sistema. Lo que si se resuelve aca es la
/// identidad, que no puede delegarse: un token autentico igual podria traer en
/// el body el id de otro cliente.
///
/// Un token ausente o mal formado deja todo en null; quien lo necesite decide
/// si eso es un error.
/// </summary>
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string SubjectClaim  = "sub";
    private const string ClientIdClaim = "client_id";
    private const string RoleClaim     = "role";
    private const string UsernameClaim = "username";

    private JwtSecurityToken? Token => ReadToken();

    public long? ClientId =>
        long.TryParse(Claim(ClientIdClaim), out var clientId) ? clientId : null;

    public long? UserId =>
        long.TryParse(Claim(SubjectClaim), out var userId) ? userId : null;

    public string? Role => Claim(RoleClaim);

    public string? Username => Claim(UsernameClaim);

    private string? Claim(string type)
        => Token?.Claims.FirstOrDefault(x => x.Type == type)?.Value;

    private JwtSecurityToken? ReadToken()
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ", StringComparison.Ordinal))
            return null;

        var raw = header["Bearer ".Length..].Trim();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.CanReadToken(raw) ? handler.ReadJwtToken(raw) : null;
        }
        catch (Exception)
        {
            // Un token ilegible no es una excepcion del negocio: es un llamador
            // sin identidad, y asi se trata.
            return null;
        }
    }
}
