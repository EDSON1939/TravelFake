using Core.Infrastructure.Grpc.Factory;

namespace Qr.Infrastructure.Grpc;

public class CommerceGrpcClient
    : GrpcClientFactory<Commerce.CommerceClient>
{
    public override string Name => "Commerce";

    public CommerceGrpcClient(
        global::Grpc.Net.ClientFactory.GrpcClientFactory grpcClientFactory)
        : base(grpcClientFactory) { }

    public Commerce.CommerceClient Client => client;
}