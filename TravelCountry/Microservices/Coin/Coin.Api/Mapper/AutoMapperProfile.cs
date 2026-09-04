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
        CreateMap<CoinResponse, CoinDataPb>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")));

        // ── GetCoins ─────────────────────────────────────────────────────────
        CreateMap<BaseResponse<List<CoinResponse>>, GetCoinsBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Create / Update ──────────────────────────────────────────────────
        CreateMap<BaseResponse<long>, CoinMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Shared ───────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}
