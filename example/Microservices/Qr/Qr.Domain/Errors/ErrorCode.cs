namespace Qr.Domain.Errors;

public struct ErrorCode
{
    public const string QR_NOT_FOUND           = nameof(QR_NOT_FOUND);
    public const string QR_DUPLICATE           = nameof(QR_DUPLICATE);
    public const string QR_NOT_ACTIVE          = nameof(QR_NOT_ACTIVE);
    public const string QR_USED                = nameof(QR_USED);
    public const string QR_EXPIRED             = nameof(QR_EXPIRED);
    public const string INSERT_FAILED          = nameof(INSERT_FAILED);
    public const string UPDATE_FAILED          = nameof(UPDATE_FAILED);
    public const string COMMERCE_NOT_FOUND     = nameof(COMMERCE_NOT_FOUND);
    public const string EXTERNAL_SERVICE_ERROR = nameof(EXTERNAL_SERVICE_ERROR);
}