using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Wcf
{
    internal class WcfLoggingBehavior : IClientMessageInspector, IEndpointBehavior
    {
        private readonly ILogger _logger;
        private const string MessageTemplate = "{TraceId} WCF {Action} responded in {Elapsed:0.0000} ms. Request: {Request} Response: {Response}";

        public WcfLoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestBody = string.Empty;

            try
            {
                if (!request.IsEmpty)
                {
                    var buffer = request.CreateBufferedCopy(int.MaxValue);
                    request = buffer.CreateMessage();
                    using var reader = buffer.CreateMessage().GetReaderAtBodyContents();
                    requestBody = reader.ReadOuterXml();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read WCF request body");
                requestBody = "[Unable to read request]";
            }

            return new WcfCallContext
            {
                Stopwatch = stopwatch,
                Action = request.Headers.Action ?? "Unknown",
                RequestBody = requestBody,
                TraceId = Activity.Current?.TraceId.ToString()
            };
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (correlationState is not WcfCallContext context)
                return;

            context.Stopwatch.Stop();
            var responseBody = string.Empty;

            try
            {
                if (!reply.IsEmpty && !reply.IsFault)
                {
                    var buffer = reply.CreateBufferedCopy(int.MaxValue);
                    reply = buffer.CreateMessage();
                    using var reader = buffer.CreateMessage().GetReaderAtBodyContents();
                    responseBody = reader.ReadOuterXml();
                }
                else if (reply.IsFault)
                {
                    responseBody = "[FAULT]";
                }

                _logger.LogInformation(MessageTemplate,
                    context.TraceId,
                    context.Action,
                    context.Stopwatch.Elapsed.TotalMilliseconds,
                    context.RequestBody,
                    responseBody);
            }
            catch (Exception exception)
            {
                _logger.LogError(MessageTemplate,
                    context.TraceId,
                    context.Action,
                    context.Stopwatch.Elapsed.TotalMilliseconds,
                    context.RequestBody,
                    exception.Message);
            }
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void Validate(ServiceEndpoint endpoint) { }

        private class WcfCallContext
        {
            public Stopwatch Stopwatch { get; set; }
            public string Action { get; set; }
            public string RequestBody { get; set; }
            public string TraceId { get; set; }
        }
    }
}
