using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Movements.Queries.GetHistory;

/// <summary>
/// Historial de transacciones del titular con los filtros del reto: rango de
/// fechas y estado, los tres opcionales.
/// </summary>
public record GetHistoryQuery(
    string    OwnerType,
    long      OwnerId,
    DateTime? DateFrom,
    DateTime? DateTo,
    string?   Status,
    int       PageNumber,
    int       PageSize) : IRequest<BaseResponse<List<MovementResponse>>>;
