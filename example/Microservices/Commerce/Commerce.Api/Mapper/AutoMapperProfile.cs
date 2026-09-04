using AutoMapper;
using Commerce.Api.Grpc;
using Commerce.Application.Common;
using Core.Domain.Models;

namespace Commerce.Api.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── GetCommerceById ─────────────────────────────────────────────────
        CreateMap<BaseResponse<CommerceResponse>, GetCommerceBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));
        CreateMap<CommerceResponse, CommerceDataPb>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.CommerceId))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
            .ForMember(d => d.CuentaId, o => o.MapFrom(s => s.CuentaId ?? 0))
            .ForMember(d => d.FechaCreacion, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
            .ForMember(d => d.FechaActualizacion,
                o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : string.Empty));

        // ── GetCommerceList ─────────────────────────────────────────────────
        CreateMap<BaseResponse<PagedResult<CommerceResponse>>, GetCommerceListBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail))
            .ForMember(d => d.Data,       o => o.MapFrom(s => s.Data!.Items))
            .ForMember(d => d.PageNumber, o => o.MapFrom(s => s.Data!.PageNumber))
            .ForMember(d => d.PageSize,   o => o.MapFrom(s => s.Data!.PageSize))
            .ForMember(d => d.TotalCount, o => o.MapFrom(s => s.Data!.TotalCount))
            .ForMember(d => d.TotalPages, o => o.MapFrom(s => s.Data!.TotalPages));

        // ── Create / Update / Activate / Inactivate ─────────────────────────
        CreateMap<BaseResponse<long>, CommerceMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Shared ──────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}
