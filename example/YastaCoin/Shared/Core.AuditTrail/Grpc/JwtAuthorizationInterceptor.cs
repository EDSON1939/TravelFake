using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Core.AuditTrail.Grpc
{
    public class JwtAuthorizationInterceptor : Interceptor
    {
        private readonly ILogger<JwtAuthorizationInterceptor> _logger;

        public JwtAuthorizationInterceptor(ILogger<JwtAuthorizationInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> next)
        {
            var metadata = context.RequestHeaders;
            var tokenHeader = metadata.GetValue("Authorization");
            if (string.IsNullOrEmpty(tokenHeader) || !tokenHeader.StartsWith("Bearer "))
            {
                var type = typeof(UnaryServerMethod<TRequest, TResponse>).GetMethod("Invoke")?.ReturnType.GenericTypeArguments[0];
                return type == null
                    ? throw new RpcException(new Status(StatusCode.Internal, "Type is not defined"))
                    : (TResponse?)Activator.CreateInstance(type, new UnauthorizedAccessException("Token de autorización inválido o caducado")) ?? default!;
            }

            var token = tokenHeader.Substring("Bearer ".Length);

            try
            {
                var valid = ValidateJwtToken(token);
                if (!valid)
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Token de autorización inválido"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar el JWT.");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token de autorización inválido o caducado"));
            }
            return await next(request, context);
        }

        private bool ValidateJwtToken(string token)
        {
            return true; 
        }
    }

}
