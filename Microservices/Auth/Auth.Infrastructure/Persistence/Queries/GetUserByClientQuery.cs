using Auth.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace Auth.Infrastructure.Persistence.Queries;

public class GetUserByClientQuery(long clientId) : QuerySingleBase<UserEntity>
{
    public override string SqlStatement => @"
        SELECT USUA_ID_IT                AS UserId,
               USUA_USERNAME_VC          AS Username,
               USUA_PASSWORD_HASH_VC     AS PasswordHash,
               USUA_CLIENTE_ID_IT        AS ClientId,
               USUA_NOMBRE_VC            AS FullName,
               USUA_ROL_VC               AS Role,
               USUA_ACTIVO_BT            AS IsActive,
               USUA_INTENTOS_FALLIDOS_IT AS FailedAttempts,
               USUA_BLOQUEADO_HASTA_DT   AS LockedUntil,
               USUA_ULTIMO_ACCESO_DT     AS LastAccessAt,
               USUA_FECHA_CREACION_DT    AS CreatedAt,
               USUA_FECHA_ACTUALIZACION_DT AS UpdatedAt,
               USUA_FECHA_ELIMINACION_DT   AS DeletedAt
        FROM   commerce.USUARIO
        WHERE  USUA_CLIENTE_ID_IT       = @ClientId
          AND  USUA_FECHA_ELIMINACION_DT IS NULL";

    public override DynamicParameters? Parameters { get; } = BuildParameters(clientId);

    private static DynamicParameters BuildParameters(long clientId)
    {
        var p = new DynamicParameters();
        p.Add("@ClientId", clientId, DbType.Int64);
        return p;
    }
}
