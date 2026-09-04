using Core.ShareKernel.Security;

namespace AccountsAndMovements.Api.Security;

/// <summary>
/// Suple el claim client_id cuando el token no lo trae, para poder probar el
/// servicio sin el microservicio de Clientes ni un login real contra Auth.
///
/// Solo se registra si el entorno es Development Y DevAuth:Enabled esta en true,
/// y nunca pisa un claim existente: si el token identifica a un cliente, ese
/// manda. Fuera de Development esta clase no llega a instanciarse, asi que un
/// appsettings de produccion con la seccion puesta por error no habilita nada.
/// </summary>
public class DevelopmentCurrentUser(ICurrentUser inner, long fallbackClientId) : ICurrentUser
{
    public long? ClientId => inner.ClientId ?? fallbackClientId;

    public long? UserId => inner.UserId;

    public string? Role => inner.Role;

    public string? Username => inner.Username;
}
