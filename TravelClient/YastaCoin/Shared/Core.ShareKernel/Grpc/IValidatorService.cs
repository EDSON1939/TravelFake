using FluentValidation;

namespace Core.ShareKernel.Grpc
{
    public interface IValidatorService
    {
        bool TryGetValidator<TRequest>(out IValidator<TRequest>? result) where TRequest : class;
    }
}