using Client.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace Client.Infrastructure.Persistence.Queries;

/// <summary>SP travelfake.GET_CLIENTE_BY_ID.</summary>
public class GetClientByIdQuery(long customerId) : QuerySingleBase<ClientEntity>
{
    public override string? Procedure => "travelfake.GET_CLIENTE_BY_ID";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@CLIENTES_ID_IT", customerId, DbType.Int64);
            return p;
        }
    }
}
