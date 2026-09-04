using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;
using Country.Domain.Entities;
using Country.Domain.Repositories;
using Country.Infrastructure.Persistence.Commands;
using Country.Infrastructure.Persistence.Queries;

namespace Country.Infrastructure.Repositories;

public class CountryRepository(IQuery query, ICommand command) : ICountryRepository
{
    public async Task<CountryEntity?> GetById(long countryId, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetCountryByIdQuery(countryId), ct);

    public async Task<IEnumerable<CountryEntity>> GetAll(bool onlyActive, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAllCountriesQuery(onlyActive), ct) ?? [];

    public async Task<long> Insert(CountryEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertCountryCommand(entity), ct);

    public async Task<long> Update(CountryEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateCountryCommand(entity), ct);

    public async Task<long> UpdateStatus(long countryId, bool isActive, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateCountryStatusCommand(countryId, isActive), ct);

    public async Task<long> Delete(long countryId, CancellationToken ct = default)
        => await command.ExecuteAsync(new DeleteCountryCommand(countryId), ct);
}
