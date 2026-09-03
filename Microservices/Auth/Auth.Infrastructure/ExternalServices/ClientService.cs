using Auth.Domain.ExternalServices;
using Core.Domain.Errors;
using Core.Infrastructure.Grpc.Factory;
using Grpc.Net.ClientFactory;

namespace Auth.Infrastructure.ExternalServices;

public class ClientService(GrpcClientFactory grpcClientFactory)
    : GrpcClientFactory<Client.Api.Grpc.Client.ClientClient>(grpcClientFactory), IClientService
{
    public override string Name => "ClientClient";

    public async Task<ClientInfo?> GetByDocument(string document, CancellationToken ct = default)
    {
        var response = await client.GetClientAsync(
            new Client.Api.Grpc.GetClientRequestPb { Document = document }, cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        return new ClientInfo(
            response.Data.ClientId,
            response.Data.Document,
            $"{response.Data.FirstName} {response.Data.LastName}",
            response.Data.IsActive);
    }
}
