using Grpc.Core;

namespace Core.Domain.Errors
{
    public struct ErrorMessage
    {
        public const string SUC000 = "OK.";
        public const string VAL001 = "Se produjeron uno o más errores de validación.";
        public const string ERR001 = "¡Oh no! Parece que hubo un problema al completar tu solicitud. Por favor, vuelve más tarde.";
        public const string PAY001 = "Ocurrió un problema al realizar el pago. Por favor, vuelve a intentar más tarde";
        public const string PAY002 = "Ocurrió un problema de comunicación. Por favor, vuelve a intentar más tarde";
        public const string AUT001 = "No existe una sesión iniciada para el usuario.";
        public const string AUT002 = "El token de seguridad no es válido.";
        public const string AUT003 = "El usuario no tiene acceso al recurso solicitado.";
    }

    public static class GrpcErrorMessage
    {
        private static IDictionary<StatusCode, string> errorMessages = new Dictionary<StatusCode, string>()
        {
            { StatusCode.Unauthenticated , ErrorMessage.AUT001 },
            { StatusCode.FailedPrecondition , ErrorMessage.AUT002 },
            { StatusCode.PermissionDenied , ErrorMessage.AUT003 }
        };

        public static IDictionary<StatusCode, string> ErrorMessages { get => errorMessages; set => errorMessages = value; }
    }
}
