using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Core.AuditTrail.Grpc
{
    public class AuthorizationInterceptor(IConfiguration configuration) : Interceptor
    {

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var type = typeof(UnaryServerMethod<TRequest, TResponse>).GetMethod("Invoke")?.ReturnType.GenericTypeArguments[0] ??
                throw new RpcException(new Status(StatusCode.Internal, "Type is not defined"));
            var token = context.RequestHeaders.FirstOrDefault(x => x.Key == "authorization")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return (TResponse?)Activator.CreateInstance(type, StatusCode.Unauthenticated) ?? default!;
            }
            var claims = ValidateToken(token.Split(" ")[1], configuration.GetValue<string>("Jwt:TokenSecret") ?? string.Empty);
            if (claims == null)
            {
                return (TResponse?)Activator.CreateInstance(type, StatusCode.FailedPrecondition) ?? default!;
            }
            return !ValidatePermissions(claims, context.Method.Split("/"))
                ? (TResponse?)Activator.CreateInstance(type, StatusCode.PermissionDenied) ?? default!
                : await base.UnaryServerHandler(request, context, continuation);
        }

        private bool ValidatePermissions(IEnumerable<Claim> claims, string[] path)
        {
            var authorizationOptions = new List<Authorization>();
            configuration.GetSection("Authorization").Bind(authorizationOptions);
            var permissions = claims.Where(x => x.Type == "permissions").Select(x => x.Value).ToList();
            if (permissions != null && authorizationOptions.Count > 0)
            {
                var method = path?.Last();
                var service = path?[^2].Split(".").Last();
                var existence = authorizationOptions.FirstOrDefault(x => x.Service == service)?.Endpoints?
                    .FirstOrDefault(x => x.Name == method)?.Permissions
                    .Any(x => permissions.Any(y => y == x));
                if (!existence.HasValue || !existence.Value)
                {
                    return false;
                }
            }
            return true;
        }

        private static IEnumerable<Claim>? ValidateToken(string token, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return ((JwtSecurityToken)validatedToken).Claims;
            }
            catch
            {
                return null;
            }
        }
    }

    public class Authorization
    {
        public string Service { get; set; } = string.Empty;

        public List<Endpoint> Endpoints { get; set; } = default!;
    }

    public class Endpoint
    {
        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = default!;
    }
}
