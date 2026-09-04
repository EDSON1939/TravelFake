using Core.Infrastructure.Database.Queries;
using Dapper;
using Qr.Domain.Entities;

namespace Qr.Infrastructure.Persistence.Queries;

public class GetQrByCodeQuery(string code) : QuerySingleBase<QrEntity>
{
    public override string SqlStatement => @"
        SELECT QR_ID_IT                  AS QrId,
               QR_CODIGO_VC              AS Codigo,
               QR_COMERCIO_ID_IT         AS ComercioId,
               QR_MONTO_DE               AS Monto,
               QR_TIPO_VC                AS Tipo,
               QR_FECHA_EXPIRACION_DT    AS FechaExpiracion,
               QR_ESTADO_VC              AS Estado,
               QR_ACTIVO_BT              AS Activo,
               QR_FECHA_CREACION_DT      AS FechaCreacion,
               QR_FECHA_ACTUALIZACION_DT AS FechaActualizacion
        FROM   qr.QR
        WHERE  QR_CODIGO_VC = @Code";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@Code", code);
            return p;
        }
    }
}