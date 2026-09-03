using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ShareKernel.Grpc
{
    public class ValidatorService(IServiceProvider provider) : IValidatorService
    {
        public bool TryGetValidator<TRequest>(out IValidator<TRequest>? result) where TRequest : class
        {
            return (result = provider?.GetService<IValidator<TRequest>>()) != null;
        }
    }
}