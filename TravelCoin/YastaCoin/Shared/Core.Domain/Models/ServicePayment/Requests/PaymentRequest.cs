namespace Core.Domain.Models.ServicePayment.Requests
{
    public class PaymentRequest
    {
        public string ChannelCode { get; set; } = string.Empty;

        public string CompanyCode { get; set; } = string.Empty;

        public string ServiceCode { get; set; } = string.Empty;

        public string Terminal { get; set; } = string.Empty;

        public string OperatorName { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string AgencyCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string ChannelTraceID { get; set; } = string.Empty;

        public string Gloss { get; set; } = string.Empty;

        public List<Criteria> SearchCriteria { get; set; } = default!;

        public string AdditionalInformation { get; set; } = string.Empty;

        public string AuthorizationCode { get; set; } = string.Empty;

        public string OperatorId { get; set; } = string.Empty;

        public string SecurityCode { get; set; } = string.Empty;

        public PaymentOption PaymentOption { get; set; } = default!;

        public BillingInformation BillingInformation { get; set; } = default!;
    }

    public class BillingInformation
    {
        public string Name { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public string DocumentNumber { get; set; } = string.Empty;

        public string DocumentComplement { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }

    public class PaymentOption
    {
        public string PaymentOptionId { get; set; } = string.Empty;

        public bool SwiftPayAccounting { get; set; }

        public string AccountNumberDebt { get; set; } = string.Empty;

        public List<PendingPaymentItem> PendingPaymentItems { get; set; } = default!;
    }

    public class PendingPaymentItem
    {
        public string DebtId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Period { get; set; } = string.Empty;

        public decimal Arrear { get; set; }
    }
}
