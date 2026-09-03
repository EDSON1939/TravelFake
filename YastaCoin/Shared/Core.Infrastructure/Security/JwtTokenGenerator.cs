using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Core.Infrastructure.Security
{
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Emite un token firmado con Jwt:TokenSecret y vigencia Jwt:TokenLifetime (minutos).
        /// Los permisos van en claims "permissions", que es lo que lee AuthorizationInterceptor.
        /// </summary>
        JwtToken Generate(string subject, IDictionary<string, string>? claims = null, IEnumerable<string>? permissions = null);
    }

    public record JwtToken(string AccessToken, DateTime ExpiresAt, int ExpiresInSeconds);

    public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
    {
        public JwtToken Generate(
            string subject,
            IDictionary<string, string>? claims = null,
            IEnumerable<string>? permissions = null)
        {
            var secret = configuration.GetValue<string>("Jwt:TokenSecret")
                ?? throw new InvalidOperationException("Falta la configuración Jwt:TokenSecret.");

            // AuthorizationInterceptor valida con Encoding.ASCII, así que firmamos igual.
            var key = Encoding.ASCII.GetBytes(secret);
            if (key.Length < 32)
                throw new InvalidOperationException("Jwt:TokenSecret debe tener al menos 32 caracteres.");

            var lifetime = configuration.GetValue<int?>("Jwt:TokenLifetime") ?? 5;
            var expiresAt = DateTime.UtcNow.AddMinutes(lifetime);

            var tokenClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, subject),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (claims is not null)
                tokenClaims.AddRange(claims.Select(x => new Claim(x.Key, x.Value)));

            if (permissions is not null)
                tokenClaims.AddRange(permissions.Select(x => new Claim("permissions", x)));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(tokenClaims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);

            return new JwtToken(handler.WriteToken(token), expiresAt, lifetime * 60);
        }
    }
}
