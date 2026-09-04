using Core.Domain.Models;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Reflection;

namespace Commerce.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any()) return await next();

        var context  = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .Select(f => new ErrorModel(f.PropertyName, f.ErrorMessage))
            .ToList();

        if (failures.Count == 0) return await next();

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(BaseResponse<>))
        {
            var method = responseType.GetMethod(
                nameof(BaseResponse<object>.ValidationError),
                BindingFlags.Static | BindingFlags.Public);

            if (method?.Invoke(null, [failures]) is TResponse validationResponse)
                return validationResponse;
        }

        throw new ValidationException(failures.Select(e => new ValidationFailure(e.Field, e.Message)));
    }
}
