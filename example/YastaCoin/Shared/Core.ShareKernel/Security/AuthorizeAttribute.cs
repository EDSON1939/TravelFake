using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.ShareKernel.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return Task.CompletedTask;
            var token = context.HttpContext.Request.Headers.Authorization.First() ??
                throw new RpcException(new Status(StatusCode.Unauthenticated, "No existe una sesión iniciada para el ususario."));
            //var claims = new JwtSecurityToken(token.Split(" ")[1]).Claims ??
            //    throw new RpcException(new Status(StatusCode.Unauthenticated, "El token de seguridad no es válido."));
            //if (!ValidatePermissions(claims, context.HttpContext.Request.Path.Value?.Split("/") ?? []))
            //{
            //    throw new RpcException(new Status(StatusCode.PermissionDenied, "El usuario no tiene acceso al recurso solicitado."));
            //}
            return Task.CompletedTask;
        }

        private bool ValidatePermissions(IEnumerable<Claim> claims, string[] path)
        {
            var configuration = new ServiceCollection().BuildServiceProvider().GetService<IConfiguration>();
            var authorizationOptions = new List<Authorization>();
            configuration.GetSection("Authorization").Bind(authorizationOptions);
            var permissions = claims.Where(x => x.Type == "permissions").Select(x => new { role = x.Value.Split("|")[0], action = x.Value.Split("|")[1] }).ToList();
            if (permissions != null && authorizationOptions.Count > 0)
            {
                var method = path?.Last();
                var service = path?[^2].Split(".").Last();
                var existence = authorizationOptions.FirstOrDefault(x => x.Service == service)?.Endpoints?
                    .FirstOrDefault(x => x.Name == method)?.Permissions
                    .Any(x => permissions.Any(y => y.role == x.Role && x.Actions.Contains(y.action)));
                if (!existence.HasValue || !existence.Value)
                {
                    return false;
                }
            }
            return true;
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

        public List<Permission> Permissions { get; set; } = default!;
    }

    public class Permission
    {
        public string Role { get; set; } = string.Empty;

        public List<string> Actions { get; set; } = default!;
    }
}
