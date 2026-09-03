using AccountsAndMovements.Domain.ExternalServices;
using Core.Domain.Errors;
using Core.Infrastructure.Grpc.Factory;
using Grpc.Net.ClientFactory;

namespace AccountsAndMovements.Infrastructure.ExternalServices;

/// <summary>
/// Cliente gRPC del microservicio de Clientes. Si Clientes no responde, el pago
/// NO puede continuar: la excepcion se propaga a proposito y el interceptor la
/// traduce a ERR001, en vez de dejar pasar un pago sin validar al titular.
/// </summary>
public class ClientService(GrpcClientFactory grpcClientFactory)
    : GrpcClientFactory<Client.Api.Grpc.Client.ClientClient>(grpcClientFactory), IClientService
{
    private const string ActiveStatus = "ACTIVE";

    public override string Name => "ClientClient";

    public async Task<ClientInfo?> GetById(long clientId, CancellationToken ct = default)
    {
        var response = await client.GetClientByIdAsync(
            new Client.Api.Grpc.GetClientByIdRequestPb { ClientId = clientId },
            cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        return new ClientInfo(
            response.Data.ClientId,
            $"{response.Data.FirstName} {response.Data.LastName}".Trim(),
            response.Data.Email,
            response.Data.CountryCode,
            response.Data.CurrencyCode.ToUpperInvariant(),
            response.Data.Status.Equals(ActiveStatus, StringComparison.OrdinalIgnoreCase));
    }
}
