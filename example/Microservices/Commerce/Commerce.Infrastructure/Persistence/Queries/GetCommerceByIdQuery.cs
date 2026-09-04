using Commerce.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;

namespace Commerce.Infrastructure.Persistence.Queries;

public class GetCommerceByIdQuery(long commerceId) : QuerySingleBase<CommerceEntity>
{
    public override string SqlStatement => @"
        SELECT COME_ID_IT                  AS CommerceId,
               COME_NOMBRE_VC              AS Name,
               COME_NIT_VC                 AS Nit,
               COME_CUENTA_ID_IT           AS CuentaId,
               COME_ACTIVO_BT              AS IsActive,
               COME_FECHA_CREACION_DT      AS CreatedAt,
               COME_FECHA_ACTUALIZACION_DT AS UpdatedAt
        FROM   commerce.COMERCIO
        WHERE  COME_ID_IT = @CommerceId";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@CommerceId", commerceId);
            return p;
        }
    }
}
