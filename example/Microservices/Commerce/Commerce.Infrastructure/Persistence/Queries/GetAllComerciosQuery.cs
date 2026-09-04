using Commerce.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;

namespace Commerce.Infrastructure.Persistence.Queries;

public class GetAllComerciosQuery(string? name, bool? onlyActive, int pageNumber, int pageSize)
    : QueryMultipleBase<CommerceEntity>
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
        WHERE  (@Name IS NULL OR COME_NOMBRE_VC LIKE '%' + @Name + '%')
          AND  (@OnlyActive IS NULL OR COME_ACTIVO_BT = @OnlyActive)
        ORDER BY COME_NOMBRE_VC ASC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@Name", string.IsNullOrWhiteSpace(name) ? null : name);
            p.Add("@OnlyActive", onlyActive.HasValue ? (onlyActive.Value ? 1 : 0) : (int?)null);
            p.Add("@Offset", (pageNumber - 1) * pageSize);
            p.Add("@PageSize", pageSize);
            return p;
        }
    }
}
