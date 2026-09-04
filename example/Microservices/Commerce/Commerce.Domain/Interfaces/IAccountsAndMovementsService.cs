using Core.Domain.Models;

namespace Commerce.Domain.Interfaces;

public interface IAccountsAndMovementsService
{
    Task<BaseResponse<long>> CreateMerchantAccount(
        long merchantId, string initialBalance, CancellationToken ct = default);
}
