using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Queries;

/// <summary>Extracto de una cuenta, mas reciente primero.</summary>
public class GetMovementsByAccountQuery(string accountNumber, int pageNumber, int pageSize)
    : QueryMultipleBase<MovementEntity>
{
    public override string SqlStatement => MovementProjection.SelectFrom + @"
        WHERE  c.CUEN_NUMERO_VC = @Number
        ORDER BY m.MOVI_FECHA_CREACION_DT DESC, m.MOVI_ID_IT DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY";

    public override DynamicParameters? Parameters { get; } = BuildParameters(accountNumber, pageNumber, pageSize);

    private static DynamicParameters BuildParameters(string accountNumber, int pageNumber, int pageSize)
    {
        var p = new DynamicParameters();
        p.Add("@Number",     accountNumber, DbType.AnsiString, size: 20);
        p.Add("@PageNumber", pageNumber,    DbType.Int32);
        p.Add("@PageSize",   pageSize,      DbType.Int32);
        return p;
    }
}
