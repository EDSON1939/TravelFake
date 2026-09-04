using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.WorkerPool.Task
{
    public abstract class GrpcCallingTask<T, U>(IConfiguration configuration, ILogger<GrpcCallingTask<T, U>> logger)
        : TaskExecution<GrpcCallingTask<T, U>, U>(configuration, logger) where T : ClientBase<T> where U : IMessage<U>
    {
        protected override async Task<U?> ExecuteTask()
        {
            var url = configuration.GetValue<string>(BaseAddressConfigKey);
            if (string.IsNullOrEmpty(url))
            {
                logger.LogError("Configuration key is not found in configuration file");
                return default;
            }
            using var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpClient = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator })
                {
                    Timeout = Timeout.InfiniteTimeSpan
                }
            });
            client = (T?)Activator.CreateInstance(typeof(T), channel);
            if (client == null)
            {
                logger.LogError("Instance could not be created");
                return default;
            }
            U response = await Method(new Empty());
            logger.LogInformation("{response}", JsonConvert.SerializeObject(response));
            return response;
        }

        public abstract string BaseAddressConfigKey { get; }

        protected T? client;

        public abstract Func<Empty, AsyncUnaryCall<U>> Method { get; }
    }
}
