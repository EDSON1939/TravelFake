using Core.Domain.Models;
using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Core.ShareKernel.Grpc
{
    public class ValidationInterceptor(IValidatorService validatorService) : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            if (validatorService.TryGetValidator(out IValidator<TRequest>? result) && result != null)
            {
                var results = await result.ValidateAsync(request);
                if (!results.IsValid && results.Errors.Count != 0)
                {
                    var errorModels = results.Errors.Select(x => new ErrorModel(x.PropertyName, x.ErrorMessage)).ToList();
                    try
                    {
                        var type = typeof(UnaryServerMethod<TRequest, TResponse>).GetMethod("Invoke")?.ReturnType.GenericTypeArguments[0];
                        return type == null
                        ? throw new RpcException(new Status(StatusCode.Internal, "Type is not defined"))
                        : (TResponse?)Activator.CreateInstance(type, errorModels) ?? default!;
                    }
                    catch (Exception exception)
                    {
                        throw new RpcException(new Status(StatusCode.Internal, "Unhandled exception", exception));
                    }
                }
                else
                {
                    return await continuation(request, context);
                }
            }
            return await continuation(request, context);
        }
    }
}
