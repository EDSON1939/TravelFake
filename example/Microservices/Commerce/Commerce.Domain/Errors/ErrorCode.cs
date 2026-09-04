namespace Commerce.Domain.Errors;

public struct ErrorCode
{
    public const string COMMERCE_NOT_FOUND = nameof(COMMERCE_NOT_FOUND);
    public const string COMMERCE_DUPLICATE = nameof(COMMERCE_DUPLICATE);
    public const string INSERT_FAILED      = nameof(INSERT_FAILED);
    public const string UPDATE_FAILED      = nameof(UPDATE_FAILED);
}
