using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Commerce.Infrastructure.Persistence.Commands;

public class UpdateCommerceStatusCommand(long commerceId, bool isActive) : SqlCommandBase<long>
{
    public override string Name => "commerce.UPDATE_COMERCIO_ESTADO";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@COME_ID_IT",      SqlDbType = SqlDbType.BigInt, Value = commerceId },
        new() { ParameterName = "@COME_ACTIVO_BT",  SqlDbType = SqlDbType.Bit,    Value = isActive },
    ];
}
