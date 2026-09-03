using Microsoft.AspNetCore.Http;

namespace Core.Infrastructure.Grpc
{
    /// <summary>
    /// Reenvía el header Authorization de la llamada entrante a las llamadas gRPC
    /// salientes. Sin esto, la cadena Gateway → Transfer → Account se corta en el
    /// primer salto: el token llega al borde y no viaja hacia adentro.
    /// </summary>
    public class TokenPropagationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        private const string AuthorizationHeader = "Authorization";

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var incoming = httpContextAccessor.HttpContext?.Request.Headers[AuthorizationHeader].FirstOrDefault();

            if (!string.IsNullOrEmpty(incoming) && !request.Headers.Contains(AuthorizationHeader))
                request.Headers.TryAddWithoutValidation(AuthorizationHeader, incoming);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
