using Dapper;
using Core.Infrastructure.Database.Queries;

namespace Commerce.Infrastructure.Persistence.Queries;

public class ExistsCommerceByNitQuery(string nit) : QuerySingleBase<bool>
{
    public override string SqlStatement => @"
        SELECT CAST(CASE WHEN EXISTS (
            SELECT 1 FROM commerce.COMERCIO WHERE COME_NIT_VC = @Nit
        ) THEN 1 ELSE 0 END AS BIT)";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@Nit", nit);
            return p;
        }
    }
}
