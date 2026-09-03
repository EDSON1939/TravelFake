using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Movements.Queries.GetMovements;

/// <summary>Extracto de una cuenta: todos sus asientos, mas reciente primero.</summary>
public record GetMovementsQuery(string AccountNumber, int PageNumber, int PageSize)
    : IRequest<BaseResponse<List<MovementResponse>>>;
