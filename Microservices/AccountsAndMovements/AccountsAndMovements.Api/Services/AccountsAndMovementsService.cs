using AccountsAndMovements.Api.Grpc;
using AccountsAndMovements.Application.Features.Accounts.Commands.ApplyMovement;
using AccountsAndMovements.Application.Features.Accounts.Commands.CreateClientAccount;
using AccountsAndMovements.Application.Features.Accounts.Commands.CreateMerchantAccount;
using AccountsAndMovements.Application.Features.Accounts.Queries.GetAccount;
using AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByOwner;
using AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByOwner;
using AccountsAndMovements.Application.Features.Movements.Queries.GetHistory;
using AccountsAndMovements.Application.Features.Movements.Queries.GetMovements;
using AccountsAndMovements.Application.Features.Payments.Commands.ExecuteQrPayment;
using AccountsAndMovements.Application.Features.Payments.Queries.GetPayment;
using AccountsAndMovements.Domain.Entities;
using AutoMapper;
using Core.ShareKernel.Security;
using Grpc.Core;
using MediatR;
using System.Globalization;

namespace AccountsAndMovements.Api.Services;

/// <summary>
/// Borde gRPC del microservicio. No decide nada: traduce el mensaje a un
/// comando o consulta de MediatR y mapea la respuesta. Las reglas viven en los
/// handlers y en los stored procedures.
/// </summary>
public class AccountsAndMovementsService(ISender sender, IMapper mapper, ICurrentUser currentUser)
    : AccountsAndMovements.Api.Grpc.AccountsAndMovements.AccountsAndMovementsBase
{
    // ── Cuentas ──────────────────────────────────────────────────────────────
    public override async Task<AccountMutationBaseResponsePb> CreateClientAccount(
        CreateClientAccountRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateClientAccountCommand(
                request.ClientId, request.CoinCode, ParseDecimal(request.InitialBalance)),
            context.CancellationToken);

        return mapper.Map<AccountMutationBaseResponsePb>(result);
    }

    public override async Task<AccountMutationBaseResponsePb> CreateMerchantAccount(
        CreateMerchantAccountRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateMerchantAccountCommand(
                request.MerchantId, ParseDecimal(request.InitialBalance)),
            context.CancellationToken);

        return mapper.Map<AccountMutationBaseResponsePb>(result);
    }

    public override async Task<GetAccountBaseResponsePb> GetAccount(
        GetAccountRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(new GetAccountQuery(request.Number), context.CancellationToken);

        return mapper.Map<GetAccountBaseResponsePb>(result);
    }

    public override async Task<GetAccountBaseResponsePb> GetMyAccount(
        GetMyAccountRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetAccountByOwnerQuery(AccountOwnerType.CLIENTE, ClientId(), request.CoinCode),
            context.CancellationToken);

        return mapper.Map<GetAccountBaseResponsePb>(result);
    }

    public override async Task<GetAccountsBaseResponsePb> GetMyAccounts(
        GetMyAccountsRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetAccountsByOwnerQuery(AccountOwnerType.CLIENTE, ClientId(), request.OnlyActive),
            context.CancellationToken);

        return mapper.Map<GetAccountsBaseResponsePb>(result);
    }

    public override async Task<GetAccountBaseResponsePb> GetMerchantAccount(
        GetMerchantAccountRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetAccountByOwnerQuery(AccountOwnerType.COMERCIO, request.MerchantId, CurrencyCode.BOB),
            context.CancellationToken);

        return mapper.Map<GetAccountBaseResponsePb>(result);
    }

    public override async Task<GetAccountsBaseResponsePb> GetMerchantAccounts(
        GetMerchantAccountsRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetAccountsByOwnerQuery(AccountOwnerType.COMERCIO, request.MerchantId, request.OnlyActive),
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
        // La identidad no se pide, se deriva del token: un cliente solo puede
        // pagar desde su propia cuenta. Un operador interno (AGENTE, ADMIN) no
        // tiene claim client_id y por eso no puede ejecutar este pago.
        var result = await sender.Send(
            new ExecuteQrPaymentCommand(
                ClientId(), request.QrCode, ParseDecimal(request.Amount),
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

    public override async Task<GetMovementsBaseResponsePb> GetMyHistory(
        GetMyHistoryRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetHistoryQuery(
                AccountOwnerType.CLIENTE, ClientId(),
                ParseDate(request.DateFrom), ParseDate(request.DateTo),
                request.Status, request.PageNumber, request.PageSize),
            context.CancellationToken);

        return mapper.Map<GetMovementsBaseResponsePb>(result);
    }

    public override async Task<GetMovementsBaseResponsePb> GetMerchantHistory(
        GetMerchantHistoryRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetHistoryQuery(
                AccountOwnerType.COMERCIO, request.MerchantId,
                ParseDate(request.DateFrom), ParseDate(request.DateTo),
                request.Status, request.PageNumber, request.PageSize),
            context.CancellationToken);

        return mapper.Map<GetMovementsBaseResponsePb>(result);
    }

    /// <summary>
    /// Cliente del token. Las operaciones sobre lo propio no reciben el id: si
    /// lo recibieran, cualquiera podria mandar el de otro.
    /// </summary>
    private long ClientId()
        => currentUser.ClientId
        ?? throw new RpcException(new Status(
            StatusCode.PermissionDenied, "El token no identifica a un cliente."));

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
