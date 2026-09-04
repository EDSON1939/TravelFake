namespace Qr.Domain.Errors;

public struct ErrorMessage
{
    public const string QR_NOT_FOUND           = "El código QR solicitado no existe.";
    public const string QR_DUPLICATE           = "No se pudo generar un código QR único. Intente nuevamente.";
    public const string QR_NOT_ACTIVE          = "El código QR no está activo.";
    public const string QR_USED                = "El código QR ya fue consumido.";
    public const string QR_EXPIRED             = "El código QR ha expirado.";
    public const string INSERT_FAILED          = "No se pudo registrar el código QR. Intente nuevamente.";
    public const string UPDATE_FAILED          = "No se pudo actualizar el código QR. Intente nuevamente.";
    public const string COMMERCE_NOT_FOUND     = "El comercio solicitado no existe.";
    public const string EXTERNAL_SERVICE_ERROR = "Ocurrió un problema de comunicación con un servicio externo. Por favor, vuelve a intentar más tarde.";
}