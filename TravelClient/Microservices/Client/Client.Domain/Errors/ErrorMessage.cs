namespace Client.Domain.Errors;

public struct ErrorMessage
{
    public const string CLIENT_NOT_FOUND  = "El cliente solicitado no existe.";
    public const string COUNTRY_NOT_FOUND = "El país indicado no existe.";
    public const string COUNTRY_INACTIVE  = "El país indicado está inactivo.";
    public const string INSERT_FAILED     = "No se pudo registrar el cliente. Intente nuevamente.";
    public const string UPDATE_FAILED     = "No se pudo actualizar el cliente. Intente nuevamente.";
    public const string DELETE_FAILED     = "No se pudo eliminar el cliente. Intente nuevamente.";

    // ── Alta del cliente: viajan en BaseResponse.Message, no son errores ──────
    // {0} es el usuario que se le creó en Auth.
    public const string CLIENT_CREATED_WITH_USER    = "Cliente registrado. Su usuario de acceso es '{0}' y la contraseña es ese mismo nombre seguido de 123.";
    public const string CLIENT_CREATED_WITHOUT_USER = "Cliente registrado, pero Auth no pudo crear el usuario '{0}'. Dele de alta manualmente en Auth.";
}
