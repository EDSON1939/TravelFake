using Commerce.Domain.Interfaces;
using Commerce.Infrastructure.Grpc;
using Core.Domain.Errors;
using Core.Domain.Models;
using Grpc.Core;

namespace Commerce.Infrastructure.ExternalServices;

public class AccountsAndMovementsService : IAccountsAndMovementsService
{
    private readonly AccountsAndMovements.AccountsAndMovementsClient _client;

    public AccountsAndMovementsService(AccountsAndMovementsGrpcClient grpcClient)
    {
        _client = grpcClient.Client;
    }

    async Task<BaseResponse<long>> IAccountsAndMovementsService.CreateCommerceAccount(
        long commerceId, string initialBalance, CancellationToken ct)
    {
        try
        {
            var response = await _client.CreateCommerceAccountAsync(
                new CreateCommerceAccountRequestPb
                {
                    CommerceId     = commerceId,
                    InitialBalance = initialBalance
                }, cancellationToken: ct);

            if (response.StatusCode == ErrorCode.SUC000)
                return BaseResponse<long>.Success(response.Data);

            if (response.Exception is not null)
                return BaseResponse<long>.Error(response.StatusCode, response.Exception.Message);

            var message = response.Errors?.FirstOrDefault()?.Message ?? "Error al crear cuenta del comercio.";
            return BaseResponse<long>.Error(response.StatusCode, message);
        }
        catch (RpcException ex)
        {
            var message = GrpcErrorMessage.ErrorMessages.TryGetValue(ex.StatusCode, out var m)
                ? m
                : ErrorMessage.ERR001;
            return BaseResponse<long>.Error(ErrorCode.ERR001, message);
        }
    }

    public async Task<AccountMutationBaseResponsePb> CreateCommerceAccount(
        CreateCommerceAccountRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.CreateCommerceAccountAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<AccountMutationBaseResponsePb> CreateClientAccount(
        CreateClientAccountRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.CreateClientAccountAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetAccountBaseResponsePb> GetAccount(
        GetAccountRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetAccountAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetAccountBaseResponsePb> GetMyAccount(
        GetMyAccountRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetMyAccountAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetAccountsBaseResponsePb> GetMyAccounts(
        GetMyAccountsRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetMyAccountsAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetAccountBaseResponsePb> GetCommerceAccount(
        GetCommerceAccountRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetCommerceAccountAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetAccountsBaseResponsePb> GetCommerceAccounts(
        GetCommerceAccountsRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetCommerceAccountsAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<ApplyMovementBaseResponsePb> ApplyMovement(
        ApplyMovementRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.ApplyMovementAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<PaymentBaseResponsePb> ExecuteQrPayment(
        ExecuteQrPaymentRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.ExecuteQrPaymentAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<PaymentBaseResponsePb> GetPayment(
        GetPaymentRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetPaymentAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetMovementsBaseResponsePb> GetMovements(
        GetMovementsRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetMovementsAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetMovementsBaseResponsePb> GetMyHistory(
        GetMyHistoryRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetMyHistoryAsync(request, headers: headers, cancellationToken: ct);
    }

    public async Task<GetMovementsBaseResponsePb> GetCommerceHistory(
        GetCommerceHistoryRequestPb request, Metadata? headers = null, CancellationToken ct = default)
    {
        return await _client.GetCommerceHistoryAsync(request, headers: headers, cancellationToken: ct);
    }
}
