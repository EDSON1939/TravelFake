using Commerce.Domain.Entities;

namespace Commerce.Domain.Repositories;

public interface ICommerceRepository
{
    Task<CommerceEntity?> GetById(long commerceId, CancellationToken ct = default);
    Task<bool> ExistsByNit(string nit, CancellationToken ct = default);
    Task<(IEnumerable<CommerceEntity> Items, int Total)> GetAll(
        string? name, bool? onlyActive, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<long> Insert(CommerceEntity entity, CancellationToken ct = default);
    Task<long> Update(CommerceEntity entity, CancellationToken ct = default);
    Task<long> UpdateStatus(long commerceId, bool isActive, CancellationToken ct = default);
}
