using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using Qr.Domain.Entities;
using System.Data;

namespace Qr.Infrastructure.Persistence.Commands;

public class InsertQrCommand(QrEntity entity) : SqlCommandBase<long>
{
    public override string Name => "qr.INSERT_QR";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@QR_CODIGO_VC",            SqlDbType = SqlDbType.VarChar,  Size = 40,       Value = entity.Codigo          },
        new() { ParameterName = "@QR_COMERCIO_ID_IT",       SqlDbType = SqlDbType.BigInt,                 Value = entity.ComercioId      },
        new() { ParameterName = "@QR_MONTO_DE",             SqlDbType = SqlDbType.Decimal, Precision = 18, Scale = 2, Value = entity.Monto },
        new() { ParameterName = "@QR_TIPO_VC",              SqlDbType = SqlDbType.VarChar,  Size = 10,      Value = entity.Tipo            },
        new() { ParameterName = "@QR_FECHA_EXPIRACION_DT",  SqlDbType = SqlDbType.DateTime,                Value = entity.FechaExpiracion },
    ];
}