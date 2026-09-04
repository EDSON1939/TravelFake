namespace Core.ShareKernel.Security;

/// <summary>
/// Identidad del llamador, tomada del JWT. Existe para que las operaciones no
/// reciban el id del titular como parametro: si el cliente viaja en el body,
/// cualquiera puede mandar el de otro y operar sobre una cuenta ajena.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Cliente del negocio asociado al token. Null en operadores internos
    /// (AGENTE, ADMIN), que no representan a un cliente.
    /// </summary>
    long? ClientId { get; }

    /// <summary>Usuario que emitio el token (claim "sub"). Identifica al actor en la bitacora.</summary>
    long? UserId { get; }

    string? Role     { get; }

    string? Username { get; }
}
