using FluentValidation;
using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ShareKernel.Grpc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGrpcValidation(this IServiceCollection services)
        {
            services.AddScoped<IValidatorService>(provider => new ValidatorService(provider));
            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var implementationTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => x.GetInterface(typeof(IValidator<>).FullName ?? string.Empty) != null
                    && !x.Name.Contains("AbstractValidator")
                    && !x.Name.Contains("InlineValidator")
                    && !x.Name.Contains("ChildRulesContainer")).ToList();
            foreach (var implementationType in implementationTypes)
            {
                var validatorType = implementationType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IValidator<>))
                    ?? throw new AggregateException(implementationType.Name + "is not implement with IValidator<>.");
                var serviceType = typeof(IValidator<>).MakeGenericType(validatorType.GetGenericArguments().First());
                services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
            }
            return services;
        }

        public static GrpcServiceOptions EnableRequestValidation(this GrpcServiceOptions options)
        {
            options.Interceptors.Add<ValidationInterceptor>();
            return options;
        }
    }
}