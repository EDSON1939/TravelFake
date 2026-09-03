using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Movements.Queries.GetMovements;

public class GetMovementsQueryHandler(IMovementRepository repository)
    : IRequestHandler<GetMovementsQuery, BaseResponse<List<MovementResponse>>>
{
    public async Task<BaseResponse<List<MovementResponse>>> Handle(
        GetMovementsQuery request, CancellationToken ct)
    {
        var entities = await repository.GetByAccount(
            request.AccountNumber.Trim().ToUpper(), request.PageNumber, request.PageSize, ct);

        return BaseResponse<List<MovementResponse>>.Success(entities.ToResponse());
    }
}
