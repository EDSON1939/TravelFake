using Core.Domain.Models;

namespace Qr.Domain.Interfaces;

public interface ICommerceService
{
    Task<BaseResponse<bool>> ExistsAsync(long commerceId, CancellationToken ct = default);
}