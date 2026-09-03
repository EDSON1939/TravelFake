using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByOwner;

public record GetAccountsByOwnerQuery(string OwnerType, long OwnerId, bool OnlyActive)
    : IRequest<BaseResponse<List<AccountResponse>>>;
