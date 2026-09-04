using Coin.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Queries.ConvertToBoliviano;

/// <summary>
/// Converts an amount expressed in coin <paramref name="CoinId"/> to bolivianos.
/// </summary>
/// <param name="Amount">Amount as invariant decimal text (e.g. "1500.50").</param>
public record ConvertToBolivianoQuery(long CoinId, string Amount)
    : IRequest<BaseResponse<ConversionResponse>>;
