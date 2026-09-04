using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Audit;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Commands;

/// <summary>
/// Crea la cuenta y, si trae saldo inicial, su asiento de apertura, todo en una
/// transaccion. Devuelve el ID o <see cref="AccountResult.DUPLICATE"/>.
/// </summary>
public class InsertAccountCommand(AccountEntity entity, IAuditContext audit) : SqlCommandBase<long>
{
    public override string Name => "commerce.INSERT_CUENTA";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@CUEN_TITULAR_TIPO_VC",  SqlDbType = SqlDbType.VarChar, Size = 20,                 Value = entity.AccountType },
        new() { ParameterName = "@CUEN_TITULAR_ID_IT",    SqlDbType = SqlDbType.BigInt,                             Value = entity.HolderId   },
        new() { ParameterName = "@CUEN_MONEDA_ID_IT",     SqlDbType = SqlDbType.BigInt,                             Value = entity.CoinId     },
        new() { ParameterName = "@CUEN_MONEDA_CODIGO_VC", SqlDbType = SqlDbType.VarChar, Size = 10,                 Value = entity.CoinCode   },
        new() { ParameterName = "@CUEN_SALDO_INICIAL_DE", SqlDbType = SqlDbType.Decimal, Precision = 18, Scale = 8, Value = entity.Balance    },
        ..AuditParameters.For(audit),
    ];
}
