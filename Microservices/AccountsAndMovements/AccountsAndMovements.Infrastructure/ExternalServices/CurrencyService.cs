using AccountsAndMovements.Domain.ExternalServices;
using Core.Domain.Errors;
using Core.Infrastructure.Grpc.Factory;
using Grpc.Net.ClientFactory;
using System.Globalization;

namespace AccountsAndMovements.Infrastructure.ExternalServices;

/// <summary>Cliente gRPC del microservicio de Monedas.</summary>
public class CurrencyService(GrpcClientFactory grpcClientFactory)
    : GrpcClientFactory<Currency.Api.Grpc.Currency.CurrencyClient>(grpcClientFactory), ICurrencyService
{
    public override string Name => "CurrencyClient";

    public async Task<CurrencyInfo?> GetByCode(string code, CancellationToken ct = default)
    {
        var response = await client.GetCurrencyAsync(
            new Currency.Api.Grpc.GetCurrencyRequestPb { Code = code }, cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        return new CurrencyInfo(
            response.Data.CurrencyId,
            response.Data.Code.ToUpperInvariant(),
            response.Data.Symbol,
            response.Data.IsActive);
    }

    /// <summary>
    /// El decimal viaja como string: un double redondearia el tipo de cambio y
    /// esa diferencia terminaria grabada en el asiento contable.
    /// </summary>
    public async Task<decimal?> GetExchangeRate(string from, string to, CancellationToken ct = default)
    {
        var response = await client.GetExchangeRateAsync(
            new Currency.Api.Grpc.GetExchangeRateRequestPb { FromCode = from, ToCode = to },
            cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        if (!decimal.TryParse(response.Data.Rate, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
            return null;

        return rate > 0 ? rate : null;
    }
}
