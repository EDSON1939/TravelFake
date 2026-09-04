using Core.Domain.Errors;
using Core.Domain.Grpc;
using Core.Domain.Models;

namespace Client.Api.Grpc
{
    // ── Constructores parciales requeridos por LoggerInterceptor y ValidationInterceptor ──
    // Cada respuesta necesita:
    //   (List<ErrorModel>)  → usado por ValidationInterceptor (VAL001)
    //   (Exception)         → usado por LoggerInterceptor     (ERR001)
    //   (StatusCode)        → usado por AuthorizationInterceptor (AUT001/AUT002/AUT003)

    public partial class GetClientBaseResponsePb
    {
        public GetClientBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetClientBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetClientBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class GetClientsBaseResponsePb
    {
        public GetClientsBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetClientsBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetClientsBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class GetCountriesBaseResponsePb
    {
        public GetCountriesBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetCountriesBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetCountriesBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class ClientMutationBaseResponsePb
    {
        public ClientMutationBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public ClientMutationBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public ClientMutationBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }
}
