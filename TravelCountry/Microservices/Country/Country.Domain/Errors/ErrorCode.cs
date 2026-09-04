namespace Country.Domain.Errors;

public struct ErrorCode
{
    public const string COUNTRY_NOT_FOUND = nameof(COUNTRY_NOT_FOUND);
    public const string COUNTRY_IN_USE    = nameof(COUNTRY_IN_USE);
    public const string INSERT_FAILED     = nameof(INSERT_FAILED);
    public const string UPDATE_FAILED     = nameof(UPDATE_FAILED);
    public const string DELETE_FAILED     = nameof(DELETE_FAILED);
}
