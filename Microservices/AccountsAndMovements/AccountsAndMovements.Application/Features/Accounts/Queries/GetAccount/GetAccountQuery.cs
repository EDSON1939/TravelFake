using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccount;

public record GetAccountQuery(string Number) : IRequest<BaseResponse<AccountResponse>>;
