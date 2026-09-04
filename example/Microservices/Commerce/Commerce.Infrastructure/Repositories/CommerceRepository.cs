using Commerce.Domain.Entities;
using Commerce.Domain.Repositories;
using Commerce.Infrastructure.Persistence.Commands;
using Commerce.Infrastructure.Persistence.Queries;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace Commerce.Infrastructure.Repositories;

public class CommerceRepository(IQuery query, ICommand command) : ICommerceRepository
{
    public async Task<CommerceEntity?> GetById(long commerceId, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetCommerceByIdQuery(commerceId), ct);

    public async Task<bool> ExistsByNit(string nit, CancellationToken ct = default)
        => await query.QuerySqlAsync(new ExistsCommerceByNitQuery(nit), ct).ConfigureAwait(false);

    public async Task<(IEnumerable<CommerceEntity> Items, int Total)> GetAll(
        string? name, bool? onlyActive, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var items = await query.QuerySqlAsync(
            new GetAllComerciosQuery(name, onlyActive, pageNumber, pageSize), ct) ?? [];

        var total = await query.QuerySqlAsync(
            new CountComerciosQuery(name, onlyActive), ct);

        return (items, total);
    }

    public async Task<long> Insert(CommerceEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertCommerceCommand(entity), ct).ConfigureAwait(false);

    public async Task<long> Update(CommerceEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateCommerceCommand(entity), ct).ConfigureAwait(false);

    public async Task<long> UpdateStatus(long commerceId, bool isActive, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateCommerceStatusCommand(commerceId, isActive), ct).ConfigureAwait(false);
}
