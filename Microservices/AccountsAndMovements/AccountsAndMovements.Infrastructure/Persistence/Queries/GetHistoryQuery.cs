using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Queries;

/// <summary>
/// Historial del titular con los filtros que pide el reto: rango de fechas y
/// estado, ambos opcionales. Los filtros nulos se neutralizan en el WHERE en vez
/// de armar el SQL por concatenacion: el plan queda estable y no hay forma de
/// inyectar nada.
/// </summary>
public class GetHistoryQuery(
    string accountType, long holderId, DateTime? from, DateTime? to,
    string? status, int pageNumber, int pageSize) : QueryMultipleBase<MovementEntity>
{
    public override string SqlStatement => MovementProjection.SelectFrom + @"
        WHERE  c.CUEN_TITULAR_TIPO_VC = @AccountType
          AND  c.CUEN_TITULAR_ID_IT   = @HolderId
          AND  (@From   IS NULL OR m.MOVI_FECHA_CREACION_DT >= @From)
          AND  (@To     IS NULL OR m.MOVI_FECHA_CREACION_DT <  @To)
          AND  (@Status IS NULL OR m.MOVI_ESTADO_VC          = @Status)
        ORDER BY m.MOVI_FECHA_CREACION_DT DESC, m.MOVI_ID_IT DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY";

    public override DynamicParameters? Parameters { get; }
        = BuildParameters(accountType, holderId, from, to, status, pageNumber, pageSize);

    private static DynamicParameters BuildParameters(
        string accountType, long holderId, DateTime? from, DateTime? to,
        string? status, int pageNumber, int pageSize)
    {
        var p = new DynamicParameters();
        p.Add("@AccountType", accountType, DbType.AnsiString, size: 20);
        p.Add("@HolderId",    holderId,    DbType.Int64);
        p.Add("@From",        from,        DbType.DateTime);
        p.Add("@To",          to,          DbType.DateTime);
        p.Add("@Status",      status,      DbType.AnsiString, size: 20);
        p.Add("@PageNumber",  pageNumber,  DbType.Int32);
        p.Add("@PageSize",    pageSize,    DbType.Int32);
        return p;
    }
}
