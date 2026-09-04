namespace Commerce.Domain.Errors;

public struct ErrorMessage
{
    public const string COMMERCE_NOT_FOUND = "El comercio solicitado no existe.";
    public const string COMMERCE_DUPLICATE = "Ya existe un comercio con ese NIT.";
    public const string INSERT_FAILED      = "No se pudo registrar el comercio. Intente nuevamente.";
    public const string UPDATE_FAILED      = "No se pudo actualizar el comercio. Intente nuevamente.";
}
