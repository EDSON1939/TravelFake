namespace Core.Domain.Models.ServicePayment.Requests
{
    public class SwiftpayTransaction
    {
        public long TransacationId { get; set; }

        public string SessionId { get; set; } = string.Empty;

        public string ChannelCode { get; set; } = string.Empty;

        public string CompanyCode { get; set; } = string.Empty;

        public string ServiceCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public decimal Arrear { get; set; }

        public string ClientCode { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string PaymentId { get; set; } = string.Empty;

        public string DebtId { get; set; } = string.Empty;

        public bool Multiple { get; set; }

        public string Gloss { get; set; } = string.Empty;

        public string RequestDate { get; set; } = string.Empty;

        public string ResponseDate { get; set; } = string.Empty;

        public string ResponseTime { get; set; } = string.Empty;

        public string AgencyBranchCode { get; set; } = string.Empty;

        public string ChannelUser { get; set; } = string.Empty;

        public string Terminal { get; set; } = string.Empty;

        public string TraceId { get; set; } = string.Empty;

        public string TrackerId { get; set; } = string.Empty;

        public string ResponseDetail { get; set; } = string.Empty;

        public string ProcessCode { get; set; } = string.Empty;
    }
}
