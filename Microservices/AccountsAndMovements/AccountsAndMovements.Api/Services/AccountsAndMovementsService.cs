using AccountsAndMovements.Api.Grpc;
using AccountsAndMovements.Application.Features.Accounts.Commands.ApplyMovement;
using AccountsAndMovements.Application.Features.Accounts.Commands.CreateAccount;
using AccountsAndMovements.Application.Features.Accounts.Queries.GetAccount;
using AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByOwner;
using AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByOwner;
using AccountsAndMovements.Application.Features.Movements.Queries.GetHistory;
using AccountsAndMovements.Application.Features.Movements.Queries.GetMovements;
using AccountsAndMovements.Application.Features.Payments.Commands.ExecuteQrPayment;
using AccountsAndMovements.Application.Features.Payments.Queries.GetPayment;
using AutoMapper;
using Grpc.Core;
using MediatR;
using System.Globalization;

namespace AccountsAndMovements.Api.Services;

/// <summary>
/// Borde gRPC del microservicio. No decide nada: traduce el mensaje a un
/// comando o consulta de MediatR y mapea la respuesta. Las reglas viven en los
/// handlers y en los stored procedures.
/// </summary>
public class AccountsAndMovementsService(ISender sender, IMapper mapper)
    : AccountsAndMovements.Api.Grpc.AccountsAndMovements.AccountsAndMovementsBase
{
    // ── Cuentas ──────────────────────────────────────────────────────────────
    public override async Task<AccountMutationBaseResponsePb> CreateAccount(
        CreateAccountRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateAccountCommand(
                request.OwnerType, request.OwnerId, request.CoinCode,
                ParseDecimal(request.InitialBalance)),
            context.CancellationToken);

        return mapper.Map<AccountMutationBaseResponsePb>(result);
    }

    public override async Task<GetAccountBaseResponsePb> GetAccount(
        GetAccountRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(new GetAccountQuery(request.Number), context.CancellationToken);

        return mapper.Map<GetAccountBaseResponsePb>(result);
    }

    public override async Task<GetAccountBaseResponsePb> GetAccountByOwner(
        GetAccountByOwnerRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetAccountByOwnerQuery(request.OwnerType, request.OwnerId, request.CoinCode),
            context.CancellationToken);

        return mapper.Map<GetAccountBaseResponsePb>(result);
    }

    public override async Task<GetAccountsBaseResponsePb> GetAccountsByOwner(
        GetAccountsByOwnerRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetAccountsByOwnerQuery(request.OwnerType, request.OwnerId, request.OnlyActive),
            context.CancellationToken);

        return mapper.Map<GetAccountsBaseResponsePb>(result);
    }

    public override async Task<ApplyMovementBaseResponsePb> ApplyMovement(
        ApplyMovementRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new ApplyMovementCommand(
                request.AccountNumber, request.Type, ParseDecimal(request.Amount),
                request.Reference, request.IdempotencyKey, request.Description),
            context.CancellationToken);

        return mapper.Map<ApplyMovementBaseResponsePb>(result);
    }

    // ── Pagos QR ─────────────────────────────────────────────────────────────
    public override async Task<PaymentBaseResponsePb> ExecuteQrPayment(
        ExecuteQrPaymentRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new ExecuteQrPaymentCommand(
                request.ClientId, request.QrCode, ParseDecimal(request.Amount),
                request.CurrencyCode, request.IdempotencyKey, request.Description),
            context.CancellationToken);

        return mapper.Map<PaymentBaseResponsePb>(result);
    }

    public override async Task<PaymentBaseResponsePb> GetPayment(
        GetPaymentRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetPaymentQuery(request.TransactionCode, request.IdempotencyKey),
            context.CancellationToken);

        return mapper.Map<PaymentBaseResponsePb>(result);
    }

    // ── Movimientos ──────────────────────────────────────────────────────────
    public override async Task<GetMovementsBaseResponsePb> GetMovements(
        GetMovementsRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetMovementsQuery(request.AccountNumber, request.PageNumber, request.PageSize),
            context.CancellationToken);

        return mapper.Map<GetMovementsBaseResponsePb>(result);
    }

    public override async Task<GetMovementsBaseResponsePb> GetHistory(
        GetHistoryRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetHistoryQuery(
                request.OwnerType, request.OwnerId,
                ParseDate(request.DateFrom), ParseDate(request.DateTo),
                request.Status, request.PageNumber, request.PageSize),
            context.CancellationToken);

        return mapper.Map<GetMovementsBaseResponsePb>(result);
    }

    /// <summary>
    /// Los montos llegan como string. Un valor ilegible se convierte en 0 y lo
    /// rechaza el validator con un mensaje de campo, que es mas util para el
    /// llamador que una excepcion de formato.
    /// </summary>
    private static decimal ParseDecimal(string value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;

    /// <summary>Fecha ISO 8601 opcional: vacia o ilegible significa "sin filtro".</summary>
    private static DateTime? ParseDate(string value)
        => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
}
