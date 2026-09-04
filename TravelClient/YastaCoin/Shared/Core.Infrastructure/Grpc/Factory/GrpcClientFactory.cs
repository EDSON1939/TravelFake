using Grpc.Net.ClientFactory;

namespace Core.Infrastructure.Grpc.Factory
{
    public abstract class GrpcClientFactory<T> where T : class
    {
        public GrpcClientFactory(GrpcClientFactory grpcClientFactory)
        {
            client = grpcClientFactory.CreateClient<T>(Name);
        }

        public abstract string Name { get; }

        protected readonly T client;
    }
}
