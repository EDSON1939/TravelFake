using Grpc.Core;

namespace Core.Domain.Errors
{
    public struct ErrorCode
    {
        public const string SUC000 = nameof(SUC000);
        public const string VAL001 = nameof(VAL001);
        public const string ERR001 = nameof(ERR001);
        public const string PAY001 = nameof(PAY001);
        public const string PAY002 = nameof(PAY002);
        public const string AUT001 = nameof(AUT001);
        public const string AUT002 = nameof(AUT002);
        public const string AUT003 = nameof(AUT003);
    }

    public static class GrpcStatusCode
    {
        private static IDictionary<StatusCode, string> statusCodes = new Dictionary<StatusCode, string>()
        {
            { StatusCode.Unauthenticated , ErrorCode.AUT001 },
            { StatusCode.FailedPrecondition , ErrorCode.AUT002 },
            { StatusCode.PermissionDenied , ErrorCode.AUT003 }
        };

        public static IDictionary<StatusCode, string> StatusCodes { get => statusCodes; set => statusCodes = value; }
    }
}
