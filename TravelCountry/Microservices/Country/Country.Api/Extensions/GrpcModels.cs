using Core.Domain.Errors;
using Core.Domain.Grpc;
using Core.Domain.Models;

namespace Country.Api.Grpc
{
    // ── Constructores parciales requeridos por LoggerInterceptor y ValidationInterceptor ──
    // Cada respuesta necesita:
    //   (List<ErrorModel>)  → usado por ValidationInterceptor (VAL001)
    //   (Exception)         → usado por LoggerInterceptor     (ERR001)
    //   (StatusCode)        → usado por AuthorizationInterceptor (AUT001/AUT002/AUT003)

    public partial class GetCountryBaseResponsePb
    {
        public GetCountryBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetCountryBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetCountryBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
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

    public partial class CountryMutationBaseResponsePb
    {
        public CountryMutationBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public CountryMutationBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public CountryMutationBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }
}
