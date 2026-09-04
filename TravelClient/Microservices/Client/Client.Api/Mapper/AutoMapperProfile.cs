using AutoMapper;
using Client.Api.Grpc;
using Client.Application.Common;
using Core.Domain.Models;

namespace Client.Api.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── GetClient ────────────────────────────────────────────────────────
        CreateMap<BaseResponse<ClientResponse>, GetClientBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // protobuf no admite string nulo: UpdatedAt viaja vacío cuando aún no se actualizó
        CreateMap<ClientResponse, ClientDataPb>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s =>
                s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : string.Empty));

        // ── GetClients ───────────────────────────────────────────────────────
        CreateMap<BaseResponse<List<ClientResponse>>, GetClientsBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── GetCountries (proxy hacia TravelCountry) ─────────────────────────
        CreateMap<BaseResponse<List<CountryResponse>>, GetCountriesBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        CreateMap<CountryResponse, CountryDataPb>();

        // ── Create / Update / UpdateStatus / Delete ──────────────────────────
        CreateMap<BaseResponse<long>, ClientMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Shared ───────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}
