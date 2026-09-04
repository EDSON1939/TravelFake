using Core.Infrastructure.Grpc.Factory;

namespace Commerce.Infrastructure.Grpc;

public class AccountsAndMovementsGrpcClient
    : GrpcClientFactory<AccountsAndMovements.AccountsAndMovementsClient>
{
    public override string Name => "AccountsAndMovements";

    public AccountsAndMovementsGrpcClient(
        global::Grpc.Net.ClientFactory.GrpcClientFactory grpcClientFactory)
        : base(grpcClientFactory) { }

    public AccountsAndMovements.AccountsAndMovementsClient Client => client;
}