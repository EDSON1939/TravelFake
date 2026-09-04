using Polly;
using Polly.Extensions.Http;

namespace Core.Infrastructure.Http.Factory.Policies
{
    public static class HttpPolicyBuilder
    {
        public static PolicyBuilder<HttpResponseMessage> GetBase() => HttpPolicyExtensions.HandleTransientHttpError();
    }
}
