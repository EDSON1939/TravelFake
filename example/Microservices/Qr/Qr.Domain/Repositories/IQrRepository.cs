using Qr.Domain.Entities;

namespace Qr.Domain.Repositories;

public interface IQrRepository
{
    Task<QrEntity?> GetById(long qrId, CancellationToken ct = default);
    Task<QrEntity?> GetByCode(string code, CancellationToken ct = default);
    Task<long> Insert(QrEntity entity, CancellationToken ct = default);
    Task<long> MarkUsed(long qrId, CancellationToken ct = default);
    Task<long> MarkExpired(CancellationToken ct = default);
}