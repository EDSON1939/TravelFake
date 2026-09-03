using AccountsAndMovements.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace AccountsAndMovements.Infrastructure.Persistence.Queries;

/// <summary>
/// Busca el asiento ya aplicado con esa clave. Es la lectura que responde el
/// resultado original ante un reintento, en vez de cobrar dos veces.
///
/// No filtra por tipo a proposito: la clave es unica en todo el sistema, asi que
/// devuelve exactamente un asiento. Filtrar por DEBITO escondria el caso en que
/// la clave se reuso para otra cosa -una recarga, por ejemplo-, y el handler
/// necesita verlo para responder DUPLICATE_TRANSACTION.
/// </summary>
public class GetMovementByIdempotencyQuery(string idempotencyKey) : QuerySingleBase<MovementEntity>
{
    public override string SqlStatement => MovementProjection.SelectFrom + @"
        WHERE  m.MOVI_IDEMPOTENCIA_VC = @IdempotencyKey";

    public override DynamicParameters? Parameters { get; } = BuildParameters(idempotencyKey);

    private static DynamicParameters BuildParameters(string idempotencyKey)
    {
        var p = new DynamicParameters();
        p.Add("@IdempotencyKey", idempotencyKey, DbType.AnsiString, size: 64);
        return p;
    }
}
