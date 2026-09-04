using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Audit;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Commands;

/// <summary>
/// Pago QR completo: debito al cliente, credito al comercio y los dos asientos
/// en una sola transaccion del motor. Se delega al SP porque la decision de
/// saldo solo es confiable con la fila de la cuenta bloqueada, y eso no se puede
/// sostener desde C#. Devuelve el ID del asiento de debito o un codigo de
/// <see cref="PaymentResult"/>.
/// </summary>
public class ExecuteQrPaymentCommand(PaymentEntity payment, IAuditContext audit) : SqlCommandBase<long>
{
    public override string Name => "commerce.EJECUTAR_PAGO_QR";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@CUENTA_ORIGEN_VC",  SqlDbType = SqlDbType.VarChar,  Size = 20,                 Value = payment.ClientAccountNumber   },
        new() { ParameterName = "@CUENTA_DESTINO_VC", SqlDbType = SqlDbType.VarChar,  Size = 20,                 Value = payment.CommerceAccountNumber },
        new() { ParameterName = "@COMERCIO_ID_IT",    SqlDbType = SqlDbType.BigInt,                              Value = payment.CommerceId            },
        new() { ParameterName = "@QR_CODIGO_VC",      SqlDbType = SqlDbType.VarChar,  Size = 64,                 Value = payment.QrCode                },
        new() { ParameterName = "@MONTO_ORIGEN_DE",   SqlDbType = SqlDbType.Decimal,  Precision = 18, Scale = 8, Value = payment.OriginalAmount        },
        new() { ParameterName = "@MONEDA_ORIGEN_VC",  SqlDbType = SqlDbType.VarChar,  Size = 10,                 Value = payment.OriginalCurrency      },
        new() { ParameterName = "@TIPO_CAMBIO_DE",    SqlDbType = SqlDbType.Decimal,  Precision = 18, Scale = 8, Value = payment.ExchangeRate          },
        new() { ParameterName = "@MONTO_DESTINO_DE",  SqlDbType = SqlDbType.Decimal,  Precision = 18, Scale = 8, Value = payment.ConvertedAmount       },
        new() { ParameterName = "@MONEDA_DESTINO_VC", SqlDbType = SqlDbType.VarChar,  Size = 10,                 Value = payment.TargetCurrency        },
        new() { ParameterName = "@REFERENCIA_VC",     SqlDbType = SqlDbType.VarChar,  Size = 64,                 Value = payment.Reference             },
        new() { ParameterName = "@IDEMPOTENCIA_VC",   SqlDbType = SqlDbType.VarChar,  Size = 64,                 Value = payment.IdempotencyKey        },
        new() { ParameterName = "@TRANSACCION_VC",    SqlDbType = SqlDbType.VarChar,  Size = 36,                 Value = payment.TransactionCode       },
        new() { ParameterName = "@DESCRIPCION_VC",    SqlDbType = SqlDbType.NVarChar, Size = 250,                Value = payment.Description           },
        ..AuditParameters.For(audit),
    ];
}
