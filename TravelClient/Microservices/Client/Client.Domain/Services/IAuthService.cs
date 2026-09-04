using Client.Domain.Entities;
using Client.Domain.Security;

namespace Client.Domain.Services;

/// <summary>
/// Puerto hacia el microservicio Auth (TravelFake\Microservices\Auth).
/// La implementación (gRPC) vive en Client.Infrastructure.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Da de alta la credencial de acceso del cliente recién registrado.
    /// </summary>
    /// <returns>
    /// El id del usuario creado, o null cuando Auth no lo aceptó (usuario duplicado,
    /// contraseña rechazada, servicio caído...). El cliente ya está grabado en ese
    /// punto, así que el registro no se deshace: solo se avisa.
    /// </returns>
    Task<long?> CreateUser(ClientEntity client, ClientCredential credential, CancellationToken ct = default);
}
