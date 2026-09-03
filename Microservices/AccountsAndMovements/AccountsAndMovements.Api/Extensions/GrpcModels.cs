using Core.Domain.Errors;
using Core.Domain.Grpc;
using Core.Domain.Models;
using Grpc.Core;

namespace AccountsAndMovements.Api.Grpc
{
    // ── Constructores parciales requeridos por LoggerInterceptor y ValidationInterceptor ──
    // Cada respuesta necesita:
    //   (List<ErrorModel>)  → usado por ValidationInterceptor (VAL001)
    //   (Exception)         → usado por LoggerInterceptor     (ERR001)
    //   (StatusCode)        → usado por AuthorizationInterceptor (AUT001/AUT002/AUT003)

    public partial class GetAccountBaseResponsePb
    {
        public GetAccountBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetAccountBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetAccountBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class GetAccountsBaseResponsePb
    {
        public GetAccountsBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetAccountsBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetAccountsBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class GetMovementsBaseResponsePb
    {
        public GetMovementsBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetMovementsBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetMovementsBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class AccountMutationBaseResponsePb
    {
        public AccountMutationBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public AccountMutationBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public AccountMutationBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class ApplyMovementBaseResponsePb
    {
        public ApplyMovementBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public ApplyMovementBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public ApplyMovementBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class PaymentBaseResponsePb
    {
        public PaymentBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public PaymentBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public PaymentBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }
}
