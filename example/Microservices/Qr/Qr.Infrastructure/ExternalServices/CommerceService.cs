using Core.Domain.Models;
using Grpc.Core;
using Qr.Domain.Errors;
using Qr.Domain.Interfaces;
using Qr.Infrastructure.Grpc;

namespace Qr.Infrastructure.ExternalServices;

public class CommerceService : ICommerceService
{
    private readonly Commerce.CommerceClient _client;

    public CommerceService(CommerceGrpcClient grpcClient)
    {
        _client = grpcClient.Client;
    }

    public async Task<BaseResponse<bool>> ExistsAsync(long commerceId, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetCommerceByIdAsync(
                new GetCommerceByIdRequestPb { Id = commerceId }, cancellationToken: ct);

            if (response.StatusCode == Core.Domain.Errors.ErrorCode.SUC000 && response.Data is not null)
                return BaseResponse<bool>.Success(true);

            if (response.Exception is not null)
                return BaseResponse<bool>.Error(response.StatusCode, response.Exception.Message);

            var message = response.Errors?.FirstOrDefault()?.Message ?? "Error al validar el comercio.";
            return BaseResponse<bool>.Error(response.StatusCode, message);
        }
        catch (RpcException ex)
        {
            var message = Core.Domain.Errors.GrpcErrorMessage.ErrorMessages.TryGetValue(ex.StatusCode, out var m)
                ? m
                : ErrorMessage.EXTERNAL_SERVICE_ERROR;
            return BaseResponse<bool>.Error(ErrorCode.EXTERNAL_SERVICE_ERROR, message);
        }
    }
}