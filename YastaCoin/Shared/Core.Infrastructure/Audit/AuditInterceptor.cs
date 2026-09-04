using Core.Infrastructure.Database.Commands.Interfaces;
using Core.ShareKernel.Security;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Core.Infrastructure.Audit
{
    /// <summary>
    /// Deja una fila en aud.BITACORA por cada RPC atendido: quien llamo, a que,
    /// desde donde y como termino. Incluye las consultas y las llamadas que
    /// terminaron mal, que son justamente las que interesan cuando alguien
    /// pregunta "que paso aca".
    ///
    /// Dos reglas que no se negocian:
    ///
    /// 1. Auditar nunca puede tumbar la operacion. Si la escritura falla, se
    ///    registra un warning y la respuesta del negocio sigue su camino. Es lo
    ///    contrario de aud.AUDITORIA, que vive dentro de la transaccion del
    ///    cambio y si tiene que arrastrarla: ahi lo que esta en juego es la
    ///    integridad del dato, aca solo la trazabilidad de la llamada.
    ///
    /// 2. Nunca se guarda una contrasena ni un token. El filtro corre antes de
    ///    escribir, no en una consulta despues.
    /// </summary>
    public class AuditInterceptor(
        ICommand command,
        ICurrentUser currentUser,
        IConfiguration configuration,
        ILogger<AuditInterceptor> logger) : Interceptor
    {
        /// <summary>Campos que se enmascaran antes de guardar la peticion.</summary>
        private static readonly Regex SensitiveFields = new(
            "\"(password|accessToken|access_token|token|secret|refreshToken|passwordHash)\"\\s*:\\s*\"[^\"]*\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // La propiedad status_code se resuelve por reflexion: los tipos que
        // genera protobuf no comparten interfaz. Se cachea por tipo para no
        // pagar la busqueda en cada llamada.
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> StatusCodeProperties = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> MessageProperties = new();

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var stopwatch = Stopwatch.StartNew();

            TResponse? response = null;
            Exception? failure  = null;

            try
            {
                response = await continuation(request, context);
                return response;
            }
            catch (Exception exception)
            {
                failure = exception;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                await Registrar(request, response, failure, context, (int)stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task Registrar<TRequest, TResponse>(
            TRequest request, TResponse? response, Exception? failure,
            ServerCallContext context, int elapsed)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                var entry = new BitacoraEntry
                {
                    Service   = configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "desconocido",
                    Method    = Recortar(context.Method, 160),
                    Operation = Operacion(context.Method),
                    UserId    = currentUser.UserId,
                    Username  = Recortar(currentUser.Username, 60),
                    ClientId  = currentUser.ClientId,
                    Role      = Recortar(currentUser.Role, 20),
                    Ip        = Direccion(context.Peer),
                    TraceId   = Activity.Current?.TraceId.ToString(),
                    Request   = Peticion(request),
                    State     = failure is null ? "OK" : "ERROR",
                    Code      = failure is null ? Recortar(Leer(response, StatusCodeProperties), 40) : "ERR001",
                    Message   = Recortar(failure?.Message ?? Leer(response, MessageProperties), 500),
                    ElapsedMilliseconds = elapsed
                };

                await command.ExecuteAsync(new InsertBitacoraCommand(entry), CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Sin rethrow: la respuesta del negocio ya se decidio y no puede
                // depender de que la bitacora se haya podido escribir.
                logger.LogWarning(exception,
                    "No se pudo registrar la bitacora de {Metodo}.", context.Method);
            }
        }

        /// <summary>
        /// Intencion de la llamada, deducida del nombre del metodo. Es gRPC, asi
        /// que el verbo del transporte siempre es POST y no dice nada.
        ///
        /// Login cae en CAMBIO a proposito: un inicio de sesion escribe -sella el
        /// ultimo acceso y mueve el contador de intentos fallidos-, no es una
        /// consulta.
        /// </summary>
        private static string Operacion(string method)
        {
            var name = method.Split('/').LastOrDefault() ?? string.Empty;

            if (name.StartsWith("Get", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("List", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Search", StringComparison.OrdinalIgnoreCase))
                return "CONSULTA";

            if (name.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Insert", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Register", StringComparison.OrdinalIgnoreCase))
                return "ALTA";

            if (name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Remove", StringComparison.OrdinalIgnoreCase))
                return "BAJA";

            return "CAMBIO";
        }

        /// <summary>
        /// El cuerpo del request en JSON, con los campos sensibles enmascarados.
        /// Un mensaje que no se pueda serializar no justifica perder la fila
        /// entera de bitacora, asi que se guarda sin peticion.
        /// </summary>
        private static string? Peticion<TRequest>(TRequest request)
        {
            if (request is not IMessage message)
                return null;

            try
            {
                return SensitiveFields.Replace(
                    JsonFormatter.Default.Format(message),
                    match => match.Value[..(match.Value.IndexOf(':') + 1)] + " \"***\"");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string? Leer<TResponse>(
            TResponse? response, ConcurrentDictionary<Type, PropertyInfo?> cache)
        {
            if (response is null)
                return null;

            var property = cache.GetOrAdd(
                response.GetType(),
                type => type.GetProperty(cache == StatusCodeProperties ? "StatusCode" : "Message"));

            return property?.GetValue(response)?.ToString();
        }

        /// <summary>
        /// context.Peer llega como "ipv4:127.0.0.1:54321". Se queda con la
        /// direccion, que es el dato util; el puerto de origen es efimero.
        /// </summary>
        private static string? Direccion(string? peer)
        {
            if (string.IsNullOrEmpty(peer))
                return null;

            var sinEsquema = peer.Contains(':') ? peer[(peer.IndexOf(':') + 1)..] : peer;
            var ultimo     = sinEsquema.LastIndexOf(':');

            return Recortar(ultimo > 0 ? sinEsquema[..ultimo] : sinEsquema, 45);
        }

        private static string? Recortar(string? value, int max)
            => string.IsNullOrEmpty(value) ? null
             : value.Length <= max ? value
             : value[..max];
    }
}
