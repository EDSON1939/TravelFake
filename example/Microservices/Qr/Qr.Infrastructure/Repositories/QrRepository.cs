using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;
using Qr.Domain.Entities;
using Qr.Domain.Repositories;
using Qr.Infrastructure.Persistence.Commands;
using Qr.Infrastructure.Persistence.Queries;

namespace Qr.Infrastructure.Repositories;

public class QrRepository(IQuery query, ICommand command) : IQrRepository
{
    public async Task<QrEntity?> GetById(long qrId, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetQrByIdQuery(qrId), ct);

    public async Task<QrEntity?> GetByCode(string code, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetQrByCodeQuery(code), ct);

    public async Task<long> Insert(QrEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertQrCommand(entity), ct);

    public async Task<long> MarkUsed(long qrId, CancellationToken ct = default)
        => await command.ExecuteAsync(new ConsumeQrCommand(qrId), ct);

    public async Task<long> MarkExpired(CancellationToken ct = default)
        => await command.ExecuteAsync(new ExpireQrsCommand(), ct);
}