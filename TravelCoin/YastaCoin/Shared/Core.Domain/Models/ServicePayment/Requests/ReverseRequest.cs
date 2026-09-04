namespace Core.Domain.Models.ServicePayment.Requests
{
    public class ReverseRequest
    {
        public string Reason { get; set; } = string.Empty;

        public string ChannelTraceId { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public SwiftpayTransaction Transaction { get; set; } = default!;
    }
}
