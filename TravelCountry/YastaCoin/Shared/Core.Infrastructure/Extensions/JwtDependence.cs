using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Core.Infrastructure.Extensions
{
    public static class JwtDependence
    {
        public static IServiceCollection AddJwtDependence(this IServiceCollection services, IConfiguration configuration)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetValue<string>("Jwt:TokenSecret"))),
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            services.AddSingleton(tokenValidationParameters);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.TokenValidationParameters = tokenValidationParameters;
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers.Append("Token-Expired", "true");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = async (context) =>
                    {
                        if (context.Request.Path.StartsWithSegments("/tenant"))
                        {
                            string accessToken = context.Request.Query["access_token"];
                            if (string.IsNullOrEmpty(accessToken) && context.Request.Headers.ContainsKey("Authorization"))
                            {
                                accessToken = context.Request.Headers["Authorization"];
                                accessToken = accessToken.Split(" ")[1];
                            }
                            if (!string.IsNullOrEmpty(accessToken))
                                context.Token = accessToken;
                        }
                    }
                };
            });
            services.AddAuthorization(x => x.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAssertion(z =>
            {
                if(z.Resource is HttpContext httpContext)
                {
                    return ValidatePermissions(z.User.Claims, httpContext.Request.Path.Value?.Split("/") ?? [], configuration);
                }
                return false;
            }).Build());
            return services;
        }

        private static bool ValidatePermissions(IEnumerable<Claim> claims, string[] path, IConfiguration configuration)
        {
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
