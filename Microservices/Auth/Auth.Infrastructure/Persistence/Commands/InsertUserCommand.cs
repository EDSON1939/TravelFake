using Auth.Domain.Entities;
using Core.Infrastructure.Audit;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Auth.Infrastructure.Persistence.Commands;

public class InsertUserCommand(UserEntity entity, IAuditContext audit) : SqlCommandBase<long>
{
    public override string Name => "commerce.INSERT_USUARIO";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@USUA_USERNAME_VC",      SqlDbType = SqlDbType.VarChar,  Size = 50,  Value = entity.Username     },
        new() { ParameterName = "@USUA_PASSWORD_HASH_VC", SqlDbType = SqlDbType.VarChar,  Size = 256, Value = entity.PasswordHash },
        new() { ParameterName = "@USUA_CLIENTE_ID_IT",    SqlDbType = SqlDbType.BigInt,               Value = (object?)entity.ClientId ?? DBNull.Value },
        new() { ParameterName = "@USUA_NOMBRE_VC",        SqlDbType = SqlDbType.NVarChar, Size = 150, Value = entity.FullName     },
        new() { ParameterName = "@USUA_ROL_VC",           SqlDbType = SqlDbType.VarChar,  Size = 20,  Value = entity.Role         },
        ..AuditParameters.For(audit),
    ];
}
