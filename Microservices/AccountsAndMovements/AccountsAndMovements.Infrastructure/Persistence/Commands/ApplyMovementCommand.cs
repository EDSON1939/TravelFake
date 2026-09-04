using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Audit;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Commands;

/// <summary>
/// Credito o debito sobre una sola cuenta. Toda la mecanica de saldo -bloqueo de
/// fila, validacion de fondos, asiento y update- vive en el SP, en una unica
/// transaccion. Devuelve el ID del asiento o un codigo de <see cref="PaymentResult"/>.
/// </summary>
public class ApplyMovementCommand(MovementEntity movement, IAuditContext audit) : SqlCommandBase<long>
{
    public override string Name => "pay.APLICAR_MOVIMIENTO";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@CUEN_NUMERO_VC",       SqlDbType = SqlDbType.VarChar,  Size = 20,                 Value = movement.AccountNumber  },
        new() { ParameterName = "@MOVI_TIPO_VC",         SqlDbType = SqlDbType.VarChar,  Size = 10,                 Value = movement.Type           },
        new() { ParameterName = "@MOVI_MONTO_DE",        SqlDbType = SqlDbType.Decimal,  Precision = 18, Scale = 8, Value = movement.Amount         },
        new() { ParameterName = "@MOVI_REFERENCIA_VC",   SqlDbType = SqlDbType.VarChar,  Size = 64,                 Value = movement.Reference      },
        new() { ParameterName = "@MOVI_IDEMPOTENCIA_VC", SqlDbType = SqlDbType.VarChar,  Size = 64,                 Value = movement.IdempotencyKey },
        new() { ParameterName = "@MOVI_DESCRIPCION_VC",  SqlDbType = SqlDbType.NVarChar, Size = 250,                Value = movement.Description    },
        ..AuditParameters.For(audit),
    ];
}
