using AutoMapper;
using Core.Domain.Models;
using Qr.Api.Grpc;
using Qr.Application.Common;
using System.Globalization;

namespace Qr.Api.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── GetQrById / GetQrByCode ─────────────────────────────────────────
        CreateMap<BaseResponse<QrResponse>, GetQrBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));
        CreateMap<QrResponse, QrDataPb>()
            .ForMember(d => d.Monto, o => o.MapFrom(s => s.Monto.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.FechaExpiracion, o => o.MapFrom(s => s.FechaExpiracion.ToString("o")))
            .ForMember(d => d.FechaCreacion, o => o.MapFrom(s => s.FechaCreacion.ToString("o")))
            .ForMember(d => d.FechaActualizacion,
                o => o.MapFrom(s => s.FechaActualizacion.HasValue ? s.FechaActualizacion.Value.ToString("o") : string.Empty))
            .ForMember(d => d.EsValido, o => o.MapFrom(s => s.EsValido));

        // ── Create / Consume ────────────────────────────────────────────────
        CreateMap<BaseResponse<long>, QrMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Shared ──────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}