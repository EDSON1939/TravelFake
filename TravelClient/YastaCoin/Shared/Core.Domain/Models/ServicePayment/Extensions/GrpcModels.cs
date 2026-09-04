using Core.Domain.Errors;
using Core.Domain.Grpc;
using Core.Domain.Models;

namespace Core.Domain
{
    public partial class PaymentResponsePb
    {
        public PaymentResponsePb(List<ErrorModel> errors)
        {
            Message = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }

        public PaymentResponsePb(System.Exception exception)
        {
            Message = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception = new ExceptionPb
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
            };
        }
    }

    public partial class SearchCustomerResponsePb
    {
        public SearchCustomerResponsePb(List<ErrorModel> errors)
        {
            Message = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }

        public SearchCustomerResponsePb(Exception exception)
        {
            Message = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception = new ExceptionPb
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
            };
        }
    }

    public partial class ReverseResponsePb
    {
        public ReverseResponsePb(List<ErrorModel> errors)
        {
            Message = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }

        public ReverseResponsePb(Exception exception)
        {
            Message = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception = new ExceptionPb
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
            };
        }
    }

    public partial class PaymentDocumentResponsePb
    {
        public PaymentDocumentResponsePb(List<ErrorModel> errors)
        {
            Message = ErrorMessage.VAL001;
            StatusCode = ErrorCode.VAL001;
            Errors.AddRange(errors.Select(x => new ErrorPb { Field = x.Field, Message = x.Message }));
        }

        public PaymentDocumentResponsePb(Exception exception)
        {
            Message = ErrorMessage.ERR001;
            StatusCode = ErrorCode.ERR001;
            Exception = new ExceptionPb
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
            };
        }
    }
}
