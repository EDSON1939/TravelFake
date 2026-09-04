using Client.Domain.Entities;
using Client.Domain.Repositories;
using Client.Infrastructure.Persistence.Commands;
using Client.Infrastructure.Persistence.Queries;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace Client.Infrastructure.Repositories;

public class ClientRepository(IQuery query, ICommand command) : IClientRepository
{
    public async Task<ClientEntity?> GetById(long customerId, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetClientByIdQuery(customerId), ct);

    public async Task<IEnumerable<ClientEntity>> GetAll(bool onlyActive, long? countryId, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAllClientsQuery(onlyActive, countryId), ct) ?? [];

    public async Task<long> Insert(ClientEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertClientCommand(entity), ct);

    public async Task<long> Update(ClientEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateClientCommand(entity), ct);

    public async Task<long> UpdateStatus(long customerId, bool isActive, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateClientStatusCommand(customerId, isActive), ct);

    public async Task<long> Delete(long customerId, CancellationToken ct = default)
        => await command.ExecuteAsync(new DeleteClientCommand(customerId), ct);
}
