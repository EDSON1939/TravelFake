using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByHolder;

public record GetAccountsByHolderQuery(string AccountType, long HolderId, bool OnlyActive)
    : IRequest<BaseResponse<List<AccountResponse>>>;
