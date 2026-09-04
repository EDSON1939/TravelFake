using Client.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace Client.Infrastructure.Persistence.Queries;

/// <summary>
/// SP travelfake.GET_CLIENTE_ALL. Al sobreescribir <see cref="QueryBase.Procedure"/>,
/// QueryBase resuelve ExecutionType = CommandType.StoredProcedure.
/// </summary>
public class GetAllClientsQuery(bool onlyActive, long? countryId) : QueryMultipleBase<ClientEntity>
{
    public override string? Procedure => "travelfake.GET_CLIENTE_ALL";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@CLIENTES_SOLO_ACTIVOS_BT", onlyActive, DbType.Boolean);
            // NULL = todos los países
            p.Add("@CLIENTES_PAIS_ID_IT", countryId, DbType.Int64);
            return p;
        }
    }
}
