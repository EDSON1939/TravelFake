using Core.Domain.Models;

namespace Commerce.Domain.Interfaces;

public interface IAccountsAndMovementsService
{
    Task<BaseResponse<long>> CreateCommerceAccount(
        long commerceId, string initialBalance, CancellationToken ct = default);
}
