using Auth.Api.Grpc;
using Auth.Application.Common;
using AutoMapper;
using Core.Domain.Models;

namespace Auth.Api.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── Login ────────────────────────────────────────────────────────────
        CreateMap<BaseResponse<LoginResponse>, LoginBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        CreateMap<LoginResponse, LoginDataPb>()
            .ForMember(d => d.ExpiresAt, o => o.MapFrom(s => s.ExpiresAt.ToString("o")))
            .ForMember(d => d.ClientId,  o => o.MapFrom(s => s.ClientId ?? 0));

        // ── CreateUser ───────────────────────────────────────────────────────
        CreateMap<BaseResponse<long>, UserMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Shared ───────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}
