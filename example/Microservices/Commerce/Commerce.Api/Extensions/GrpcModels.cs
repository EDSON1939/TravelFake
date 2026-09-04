using Core.Domain.Errors;
using Core.Domain.Grpc;
using Core.Domain.Models;

namespace Commerce.Api.Grpc
{
    public partial class GetCommerceBaseResponsePb
    {
        public GetCommerceBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetCommerceBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetCommerceBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class GetCommerceListBaseResponsePb
    {
        public GetCommerceListBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public GetCommerceListBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public GetCommerceListBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }

    public partial class CommerceMutationBaseResponsePb
    {
        public CommerceMutationBaseResponsePb(List<ErrorModel> errors)
        {
            Message    = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }
        public CommerceMutationBaseResponsePb(Exception exception)
        {
            Message    = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception  = new ExceptionDetailPb { Message = exception.Message };
        }
        public CommerceMutationBaseResponsePb(global::Grpc.Core.StatusCode statusCode)
        {
            Message    = GrpcErrorMessage.ErrorMessages[statusCode];
            StatusCode = GrpcStatusCode.StatusCodes[statusCode];
        }
    }
}
