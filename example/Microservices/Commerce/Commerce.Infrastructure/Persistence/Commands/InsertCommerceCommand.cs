using Commerce.Domain.Entities;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Commerce.Infrastructure.Persistence.Commands;

public class InsertCommerceCommand(CommerceEntity entity) : SqlCommandBase<long>
{
    public override string Name => "commerce.INSERT_COMERCIO";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@COME_NOMBRE_VC",    SqlDbType = SqlDbType.NVarChar, Size = 50, Value = entity.Name },
        new() { ParameterName = "@COME_NIT_VC",       SqlDbType = SqlDbType.VarChar,  Size = 13, Value = entity.Nit  },
    ];
}
