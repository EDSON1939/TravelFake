using Core.Infrastructure.Database.Queries;
using Dapper;

namespace Commerce.Infrastructure.Persistence.Queries;

public class CountComerciosQuery(string? name, bool? onlyActive) : QuerySingleBase<int>
{
    public override string SqlStatement => @"
        SELECT COUNT(*)
        FROM   commerce.COMERCIO
        WHERE  (@Name IS NULL OR COME_NOMBRE_VC LIKE '%' + @Name + '%')
          AND  (@OnlyActive IS NULL OR COME_ACTIVO_BT = @OnlyActive)";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@Name", string.IsNullOrWhiteSpace(name) ? null : name);
            p.Add("@OnlyActive", onlyActive.HasValue ? (onlyActive.Value ? 1 : 0) : (int?)null);
            return p;
        }
    }
}
