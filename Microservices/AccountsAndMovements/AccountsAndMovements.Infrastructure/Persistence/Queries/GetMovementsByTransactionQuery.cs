using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Queries;

/// <summary>
/// Los dos asientos de una operacion. El DEBITO del cliente sale primero porque
/// es el que describe el pago; el CREDITO del comercio es su contrapartida.
/// </summary>
public class GetMovementsByTransactionQuery(string transactionCode) : QueryMultipleBase<MovementEntity>
{
    public override string SqlStatement => MovementProjection.SelectFrom + @"
        WHERE  m.MOVI_TRANSACCION_VC = @TransactionCode
        ORDER BY CASE WHEN m.MOVI_TIPO_VC = 'DEBITO' THEN 0 ELSE 1 END, m.MOVI_ID_IT";

    public override DynamicParameters? Parameters { get; } = BuildParameters(transactionCode);

    private static DynamicParameters BuildParameters(string transactionCode)
    {
        var p = new DynamicParameters();
        p.Add("@TransactionCode", transactionCode, DbType.AnsiString, size: 36);
        return p;
    }
}
