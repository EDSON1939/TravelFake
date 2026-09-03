using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Auth.Infrastructure.Persistence.Commands;

public class DeleteUserCommand(long userId) : SqlCommandBase<long>
{
    public override string Name => "coin.DELETE_USUARIO";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@USUA_ID_IT", SqlDbType = SqlDbType.BigInt, Value = userId },
    ];
}
