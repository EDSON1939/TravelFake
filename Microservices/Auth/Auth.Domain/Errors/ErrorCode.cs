namespace Auth.Domain.Errors;

public struct ErrorCode
{
    public const string INVALID_CREDENTIALS = nameof(INVALID_CREDENTIALS);
    public const string USER_INACTIVE       = nameof(USER_INACTIVE);
    public const string USER_LOCKED         = nameof(USER_LOCKED);
    public const string USER_DUPLICATE      = nameof(USER_DUPLICATE);
    public const string INSERT_FAILED       = nameof(INSERT_FAILED);
    public const string DELETE_FAILED       = nameof(DELETE_FAILED);
    public const string USER_NOT_FOUND      = nameof(USER_NOT_FOUND);

    // ── Vínculo con el cliente del negocio ───────────────────────────────────
    public const string CLIENT_NOT_FOUND        = nameof(CLIENT_NOT_FOUND);
    public const string CLIENT_INACTIVE         = nameof(CLIENT_INACTIVE);
    public const string CLIENT_ALREADY_HAS_USER = nameof(CLIENT_ALREADY_HAS_USER);
}
