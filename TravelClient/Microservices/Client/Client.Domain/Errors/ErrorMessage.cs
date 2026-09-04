namespace Client.Domain.Errors;

public struct ErrorMessage
{
    public const string CLIENT_NOT_FOUND  = "El cliente solicitado no existe.";
    public const string COUNTRY_NOT_FOUND = "El país indicado no existe.";
    public const string COUNTRY_INACTIVE  = "El país indicado está inactivo.";
    public const string INSERT_FAILED     = "No se pudo registrar el cliente. Intente nuevamente.";
    public const string UPDATE_FAILED     = "No se pudo actualizar el cliente. Intente nuevamente.";
    public const string DELETE_FAILED     = "No se pudo eliminar el cliente. Intente nuevamente.";
}
