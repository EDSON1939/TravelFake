using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Queries;

/// <summary>
/// Traduce (titular, moneda) al numero de cuenta que necesita el pago. Filtra
/// por codigo de moneda y no por ID para no obligar al llamador a resolver antes
/// el catalogo de Monedas.
/// </summary>
public class GetAccountByOwnerAndCoinQuery(string ownerType, long ownerId, string coinCode)
    : QuerySingleBase<AccountEntity>
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
        WHERE  CUEN_TITULAR_TIPO_VC      = @OwnerType
          AND  CUEN_TITULAR_ID_IT        = @OwnerId
          AND  CUEN_MONEDA_CODIGO_VC     = @CoinCode
          AND  CUEN_FECHA_ELIMINACION_DT IS NULL";

    public override DynamicParameters? Parameters { get; } = BuildParameters(ownerType, ownerId, coinCode);

    private static DynamicParameters BuildParameters(string ownerType, long ownerId, string coinCode)
    {
        var p = new DynamicParameters();
        p.Add("@OwnerType", ownerType, DbType.AnsiString, size: 20);
        p.Add("@OwnerId",   ownerId,   DbType.Int64);
        p.Add("@CoinCode",  coinCode,  DbType.AnsiString, size: 10);
        return p;
    }
}
