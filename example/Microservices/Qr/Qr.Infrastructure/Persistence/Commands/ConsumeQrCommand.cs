using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Qr.Infrastructure.Persistence.Commands;

public class ConsumeQrCommand(long qrId) : SqlCommandBase<long>
{
    public override string Name => "qr.UPDATE_QR_CONSUMIDO";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@QR_ID_IT", SqlDbType = SqlDbType.BigInt, Value = qrId },
    ];
}