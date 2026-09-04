using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.Repositories;
using AccountsAndMovements.Infrastructure.Persistence.Commands;
using AccountsAndMovements.Infrastructure.Persistence.Queries;
using Core.Infrastructure.Audit;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace AccountsAndMovements.Infrastructure.Repositories;

public class MovementRepository(IQuery query, ICommand command, IAuditContext audit) : IMovementRepository
{
    public async Task<IEnumerable<MovementEntity>> GetByAccount(
        string accountNumber, int pageNumber, int pageSize, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetMovementsByAccountQuery(accountNumber, pageNumber, pageSize), ct) ?? [];

    public async Task<IEnumerable<MovementEntity>> GetHistory(
        string ownerType, long ownerId, DateTime? from, DateTime? to,
        string? status, int pageNumber, int pageSize, CancellationToken ct = default)
        => await query.QuerySqlAsync(
            new GetHistoryQuery(ownerType, ownerId, from, to, status, pageNumber, pageSize), ct) ?? [];

    public async Task<MovementEntity?> GetByIdempotencyKey(string idempotencyKey, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetMovementByIdempotencyQuery(idempotencyKey), ct);

    public async Task<IEnumerable<MovementEntity>> GetByTransactionCode(
        string transactionCode, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetMovementsByTransactionQuery(transactionCode), ct) ?? [];

    public async Task<long> ApplyMovement(MovementEntity movement, CancellationToken ct = default)
        => await command.ExecuteAsync(new ApplyMovementCommand(movement, audit), ct);

    public async Task<long> ExecuteQrPayment(PaymentEntity payment, CancellationToken ct = default)
        => await command.ExecuteAsync(new ExecuteQrPaymentCommand(payment, audit), ct);
}
