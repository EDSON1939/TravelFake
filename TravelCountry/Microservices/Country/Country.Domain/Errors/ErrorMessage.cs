namespace Country.Domain.Errors;

public struct ErrorMessage
{
    public const string COUNTRY_NOT_FOUND = "El país solicitado no existe.";
    public const string COUNTRY_IN_USE    = "El país tiene clientes asociados y no puede eliminarse.";
    public const string INSERT_FAILED     = "No se pudo registrar el país. Intente nuevamente.";
    public const string UPDATE_FAILED     = "No se pudo actualizar el país. Intente nuevamente.";
    public const string DELETE_FAILED     = "No se pudo eliminar el país. Intente nuevamente.";
}
