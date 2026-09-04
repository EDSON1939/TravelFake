using Client.Domain.Entities;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Client.Infrastructure.Persistence.Commands;

/// <summary>
/// SP travelfake.UPDATE_CLIENTE — devuelve las filas afectadas,
/// o -1 si el país indicado no existe.
/// </summary>
public class UpdateClientCommand(ClientEntity entity) : SqlCommandBase<long>
{
    public override string Name => "travelfake.UPDATE_CLIENTE";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@CLIENTES_ID_IT",       SqlDbType = SqlDbType.BigInt,                Value = entity.CustomerId },
        new() { ParameterName = "@CLIENTES_NOMBRE_VC",   SqlDbType = SqlDbType.NVarChar, Size = 100,  Value = entity.FirstName  },
        new() { ParameterName = "@CLIENTES_APELLIDO_VC", SqlDbType = SqlDbType.NVarChar, Size = 100,  Value = entity.LastName   },
        new() { ParameterName = "@CLIENTES_EMAIL_VC",    SqlDbType = SqlDbType.NVarChar, Size = 100,  Value = entity.Email      },
        new() { ParameterName = "@CLIENTES_TELEFONO_VC", SqlDbType = SqlDbType.NVarChar, Size = 100,  Value = entity.Phone      },
        new() { ParameterName = "@CLIENTES_PAIS_ID_IT",  SqlDbType = SqlDbType.BigInt,                Value = entity.CountryId  },
        new() { ParameterName = "@CLIENTES_ACTIVO_BT",   SqlDbType = SqlDbType.Bit,                   Value = entity.IsActive   },
    ];
}
