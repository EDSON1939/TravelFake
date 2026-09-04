using AccountsAndMovements.Api.Grpc;
using AccountsAndMovements.Application.Common;
using AutoMapper;
using Core.Domain.Models;
using System.Globalization;

namespace AccountsAndMovements.Api.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── Cuentas ──────────────────────────────────────────────────────────
        CreateMap<BaseResponse<AccountResponse>, GetAccountBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        CreateMap<BaseResponse<List<AccountResponse>>, GetAccountsBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // Los decimales viajan como string para no perder precisión en el cable.
        CreateMap<AccountResponse, AccountDataPb>()
            .ForMember(d => d.Balance,   o => o.MapFrom(s => s.Balance.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
            // Las fechas nulas viajan como string vacío: proto3 no tiene null
            // para escalares, y un "" es inequívoco frente a una fecha ISO 8601.
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : string.Empty))
            .ForMember(d => d.DeletedAt, o => o.MapFrom(s => s.DeletedAt.HasValue ? s.DeletedAt.Value.ToString("o") : string.Empty));

        // ── Movimientos ──────────────────────────────────────────────────────
        CreateMap<BaseResponse<List<MovementResponse>>, GetMovementsBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        CreateMap<MovementResponse, MovementDataPb>()
            .ForMember(d => d.Amount,           o => o.MapFrom(s => s.Amount.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.BalanceBefore,    o => o.MapFrom(s => s.BalanceBefore.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.BalanceAfter,     o => o.MapFrom(s => s.BalanceAfter.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.OriginalAmount,   o => o.MapFrom(s => s.OriginalAmount.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.ExchangeRate,     o => o.MapFrom(s => s.ExchangeRate.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.ConvertedAmount,  o => o.MapFrom(s => s.ConvertedAmount.ToString(CultureInfo.InvariantCulture)))
            // proto3 no admite escalares nulos: 0 y "" son los equivalentes de
            // "este asiento no vino de un pago QR".
            .ForMember(d => d.CommerceId,       o => o.MapFrom(s => s.CommerceId ?? 0))
            .ForMember(d => d.QrCode,           o => o.MapFrom(s => s.QrCode ?? string.Empty))
            .ForMember(d => d.CreatedAt,        o => o.MapFrom(s => s.CreatedAt.ToString("o")));

        CreateMap<BaseResponse<MovementAppliedResponse>, ApplyMovementBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        CreateMap<MovementAppliedResponse, MovementAppliedDataPb>()
            .ForMember(d => d.Balance, o => o.MapFrom(s => s.Balance.ToString(CultureInfo.InvariantCulture)));

        // ── Pagos ────────────────────────────────────────────────────────────
        CreateMap<BaseResponse<PaymentResponse>, PaymentBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        CreateMap<PaymentResponse, PaymentDataPb>()
            .ForMember(d => d.OriginalAmount,  o => o.MapFrom(s => s.OriginalAmount.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.ExchangeRate,    o => o.MapFrom(s => s.ExchangeRate.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.ConvertedAmount, o => o.MapFrom(s => s.ConvertedAmount.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.ClientBalance,   o => o.MapFrom(s => s.ClientBalance.ToString(CultureInfo.InvariantCulture)))
            .ForMember(d => d.CreatedAt,       o => o.MapFrom(s => s.CreatedAt.ToString("o")));

        // ── Mutaciones (devuelven el ID generado) ────────────────────────────
        CreateMap<BaseResponse<long>, AccountMutationBaseResponsePb>()
            .ForMember(d => d.Exception, o => o.MapFrom(s => s.ExceptionDetail));

        // ── Shared ───────────────────────────────────────────────────────────
        CreateMap<ErrorModel, Core.Domain.Grpc.ErrorPb>();
        CreateMap<Exception, Core.Domain.Grpc.ExceptionDetailPb>()
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message));
    }
}
