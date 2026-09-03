using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Queries;

public class GetAccountByNumberQuery(string number) : QuerySingleBase<AccountEntity>
{
    public override string SqlStatement => @"
        SELECT CUEN_ID_IT                  AS AccountId,
               CUEN_NUMERO_VC              AS Number,
               CUEN_TITULAR_TIPO_VC        AS OwnerType,
               CUEN_TITULAR_ID_IT          AS OwnerId,
               CUEN_MONEDA_ID_IT           AS CoinId,
               CUEN_MONEDA_CODIGO_VC       AS CoinCode,
               CUEN_SALDO_DE               AS Balance,
               CUEN_ACTIVO_BT              AS IsActive,
               CUEN_FECHA_CREACION_DT      AS CreatedAt,
               CUEN_FECHA_ACTUALIZACION_DT AS UpdatedAt,
               CUEN_FECHA_ELIMINACION_DT   AS DeletedAt
        FROM   pay.CUENTA
        WHERE  CUEN_NUMERO_VC            = @Number
          AND  CUEN_FECHA_ELIMINACION_DT IS NULL";

    public override DynamicParameters? Parameters { get; } = BuildParameters(number);

    private static DynamicParameters BuildParameters(string number)
    {
        var p = new DynamicParameters();
        p.Add("@Number", number, DbType.AnsiString, size: 20);
        return p;
    }
}
