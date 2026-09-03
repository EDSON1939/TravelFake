namespace Core.Infrastructure.Http.Factory
{
    public class ConnectionConfig : ICircuitBreaker, IRetry
    {
        public int RetryCount { get; set; }

        public int MedianFirstRetryDelay { get; set; }

        public int BreakDuration { get; set; }
    }

    public interface ICircuitBreaker
    {
        int RetryCount { get; }

        int BreakDuration { get; }
    }

    public interface IRetry
    {
        int RetryCount { get; }

        int MedianFirstRetryDelay { get; }
    }
}
