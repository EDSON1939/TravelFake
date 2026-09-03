using Core.Domain.Errors;
using Core.Domain.Grpc;
using Core.Domain.Models;
using Grpc.Core;

namespace Auth.Api.Grpc
{
    // ── Constructores parciales requeridos por LoggerInterceptor y ValidationInterceptor ──
    // Cada respuesta necesita:
    //   (List<ErrorModel>)  → usado por ValidationInterceptor (VAL001)
    //   (Exception)         → usado por LoggerInterceptor     (ERR001)
    //   (StatusCode)        → usado por AuthorizationInterceptor (AUT001/AUT002/AUT003)

    public partial class LoginBaseResponsePb
    {
        public LoginBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public LoginBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public LoginBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class UserMutationBaseResponsePb
    {
        public UserMutationBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public UserMutationBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public UserMutationBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }
}
