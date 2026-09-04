using Grpc.Core;
using Grpc.Core.Interceptors;
using Serilog.Core;
using System.Diagnostics;

namespace Core.AuditTrail.Grpc
{
    public class LoggerInterceptor(Logger logger) : Interceptor
    {
        private const string MessageTemplate = "{TraceId} {RequestMethod} responded {StatusCode} in {Elapsed:0.0000} ms. Request: {Request} Response: {Response}";

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await base.UnaryServerHandler(request, context, continuation);
                stopwatch.Stop();
                logger.Information(MessageTemplate, Activity.Current?.TraceId ,context.Method, context.Status.StatusCode, stopwatch.Elapsed.TotalMilliseconds, request, response);
                return response;
            }
            catch(RpcException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.Error(MessageTemplate, Activity.Current?.TraceId, context.Method, context.Status.StatusCode, 0, request, exception);
                try
                {
                    var type = typeof(UnaryServerMethod<TRequest, TResponse>).GetMethod("Invoke")?.ReturnType.GenericTypeArguments[0];
                    return type == null
                        ? throw new RpcException(new Status(StatusCode.Internal, "Type is not defined"))
                        : (TResponse?)Activator.CreateInstance(type, exception) ?? default!;
                }
                catch (Exception ex)
                {
                    logger.Error(MessageTemplate, Activity.Current?.TraceId, context.Method, context.Status.StatusCode, 0, request, ex);
                    throw new RpcException(new Status(StatusCode.Internal, "Unhandled exception", ex));
                }
            }
        }
    }
}
