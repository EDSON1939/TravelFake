namespace Auth.Domain.Errors;

public struct ErrorMessage
{
    // Mismo mensaje para usuario inexistente y contraseña incorrecta: si fueran
    // distintos, un atacante podría enumerar qué usuarios existen.
    public const string INVALID_CREDENTIALS = "Usuario o contraseña incorrectos.";
    public const string USER_INACTIVE       = "El usuario se encuentra inactivo.";
    public const string USER_LOCKED         = "El usuario está bloqueado temporalmente por intentos fallidos.";
    public const string USER_DUPLICATE      = "Ya existe un usuario con ese nombre.";
    public const string INSERT_FAILED       = "No se pudo crear el usuario. Intente nuevamente.";
    public const string DELETE_FAILED       = "No se pudo eliminar el usuario: no existe o ya fue eliminado.";
    public const string USER_NOT_FOUND      = "El usuario solicitado no existe.";

    // ── Vínculo con el cliente del negocio ───────────────────────────────────
    public const string CLIENT_NOT_FOUND        = "El cliente indicado no existe.";
    public const string CLIENT_INACTIVE         = "El cliente indicado se encuentra inactivo.";
    public const string CLIENT_ALREADY_HAS_USER = "El cliente ya tiene un usuario asignado.";
}
