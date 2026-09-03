using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Movements.Queries.GetHistory;

public class GetHistoryQueryHandler(IMovementRepository repository)
    : IRequestHandler<GetHistoryQuery, BaseResponse<List<MovementResponse>>>
{
    public async Task<BaseResponse<List<MovementResponse>>> Handle(
        GetHistoryQuery request, CancellationToken ct)
    {
        var entities = await repository.GetHistory(
            request.OwnerType.Trim().ToUpper(),
            request.OwnerId,
            request.DateFrom,
            ExclusiveUpperBound(request.DateTo),
            string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper(),
            request.PageNumber,
            request.PageSize,
            ct);

        return BaseResponse<List<MovementResponse>>.Success(entities.ToResponse());
    }

    /// <summary>
    /// La consulta filtra con "menor que", asi que una fecha final sin hora
    /// -que es como la manda una app- dejaria fuera todo lo del ultimo dia. Se
    /// corre al dia siguiente para que el rango se lea como el usuario espera.
    /// </summary>
    private static DateTime? ExclusiveUpperBound(DateTime? dateTo)
        => dateTo.HasValue && dateTo.Value.TimeOfDay == TimeSpan.Zero
            ? dateTo.Value.AddDays(1)
            : dateTo;
}
