using AutoMapper;
using Coin.Api.Grpc;
using Coin.Application.Common;
using Core.Domain.Models;

namespace Coin.Api.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── GetCoin ──────────────────────────────────────────────────────────
        CreateMap<BaseResponse<CoinResponse>, GetCoinBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // Decimals go out as invariant strings and dates as ISO-8601.
        // protobuf does not allow null strings: UpdatedAt travels empty when not set yet.
        CreateMap<CoinResponse, CoinDataPb>()
            .ForMember(d => d.BuyRate,   o => o.MapFrom(s => MoneyRules.Format(s.BuyRate)))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s =>
                s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : string.Empty));

        // ── GetCoins ─────────────────────────────────────────────────────────
        CreateMap<BaseResponse<List<CoinResponse>>, GetCoinsBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Create / Update / UpdateStatus / Delete ──────────────────────────
        CreateMap<BaseResponse<long>, CoinMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── ConvertToBoliviano ───────────────────────────────────────────────
        CreateMap<BaseResponse<ConversionResponse>, ConvertToBolivianoBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        CreateMap<ConversionResponse, ConversionDataPb>()
            .ForMember(d => d.ExchangeRate, o => o.MapFrom(s => MoneyRules.Format(s.ExchangeRate)))
            .ForMember(d => d.Amount,       o => o.MapFrom(s => MoneyRules.Format(s.Amount)))
            .ForMember(d => d.AmountInBs,   o => o.MapFrom(s => MoneyRules.Format(s.AmountInBs)));

        // ── Shared ───────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}
