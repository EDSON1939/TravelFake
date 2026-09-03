using Core.Domain.Errors;

namespace Core.Domain.Models
{
    public class BaseResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; } = ErrorMessage.SUC000;

        public string StatusCode { get; set; } = ErrorCode.SUC000;

        public List<ErrorModel>? Errors { get; set; }

        public Exception? ExceptionDetail { get; set; }

        public static BaseResponse<T> Success(T? data, string code = ErrorCode.SUC000, string message = ErrorMessage.SUC000) => CreateBaseResponse(code, message, data);

        public static BaseResponse<T> ValidationError(List<ErrorModel> errors) => CreateBaseResponse(ErrorCode.VAL001, errors: errors, message: ErrorMessage.VAL001);

        public static BaseResponse<T> Error(string code, string message) => CreateBaseResponse(code, message: message);

        public static BaseResponse<T> Exception(Exception exception) => CreateBaseResponse(ErrorCode.ERR001, exception: exception, message: ErrorMessage.ERR001);

        private static BaseResponse<T> CreateBaseResponse(string code, string message, T? data = default!, Exception? exception = null, List<ErrorModel>? errors = null)
        {
            return new BaseResponse<T>
            {
                Data = data,
                Message = message,
                StatusCode = code,
                ExceptionDetail = exception,
                Errors = errors
            };
        }
    }

    public class ErrorModel(string field, string message)
    {
        public string Field { get; set; } = field;

        public string Message { get; set; } = message;
    }

    public static class ResponseExtension
    {
        public static bool IsSuccess<T>(this BaseResponse<T> response) => response.StatusCode == ErrorCode.SUC000 && response.Data != null;
    }
}
