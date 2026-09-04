using AutoMapper;
using Core.Domain.Models;
using Country.Api.Grpc;
using Country.Application.Common;

namespace Country.Api.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── GetCountry ───────────────────────────────────────────────────────
        CreateMap<BaseResponse<CountryResponse>, GetCountryBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // protobuf no admite string nulo: UpdatedAt viaja vacío cuando aún no se actualizó
        CreateMap<CountryResponse, CountryDataPb>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s =>
                s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : string.Empty));

        // ── GetCountries ─────────────────────────────────────────────────────
        CreateMap<BaseResponse<List<CountryResponse>>, GetCountriesBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Create / Update / UpdateStatus / Delete ──────────────────────────
        CreateMap<BaseResponse<long>, CountryMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Shared ───────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}
