using Core.Infrastructure.Audit;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Auth.Infrastructure.Persistence.Commands;

public class RegisterLoginAttemptCommand(
    long userId, bool succeeded, int maxAttempts, int lockMinutes, IAuditContext audit)
    : SqlCommandBase<long>
{
    public override string Name => "commerce.REGISTRAR_INTENTO_LOGIN";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@USUA_ID_IT",     SqlDbType = SqlDbType.BigInt, Value = userId      },
        new() { ParameterName = "@Exitoso",        SqlDbType = SqlDbType.Bit,    Value = succeeded   },
        new() { ParameterName = "@MaxIntentos",    SqlDbType = SqlDbType.Int,    Value = maxAttempts },
        new() { ParameterName = "@MinutosBloqueo", SqlDbType = SqlDbType.Int,    Value = lockMinutes },
        ..AuditParameters.For(audit),
    ];
}
