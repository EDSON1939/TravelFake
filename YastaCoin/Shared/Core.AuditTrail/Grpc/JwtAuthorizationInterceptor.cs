using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            // Los RPC marcados con [AllowAnonymous] no exigen token. gRPC copia
            // los atributos del metodo a la metadata del endpoint, asi que la
            // excepcion queda escrita al lado del metodo y no en una lista de
            // nombres que nadie actualiza cuando se renombra un RPC.
            if (context.GetHttpContext().GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                return await next(request, context);
            }

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
