namespace Core.Domain.Models.ServicePayment.Responses
{
    public class CustomerSearchResponse
    {
        public string TrackingID { get; set; } = string.Empty;

        public string AuthorizationCode { get; set; } = string.Empty;

        public string AdditionalInformation { get; set; } = string.Empty;

        public List<PaymentOption> PaymentOptions { get; set; } = default!;

        public List<SearchCriteria> SearchCriteria { get; set; } = default!;
    }

    public class PaymentOption
    {
        public List<Models.BillingCriteria> BillingCriteria { get; set; } = default!;

        public string PaymentOptionId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal TotalMora { get; set; }

        public decimal TotalAmount { get; set; }

        public List<PendingPaymentItem> PendingPaymentItems { get; set; } = default!;
    }

    public class PendingPaymentItem
    {
        public string DebtId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal AmountMora { get; set; }

        public decimal MinimumPaymentAmount { get; set; }

        public bool AmountTextEdit { get; set; }

        public string ExpirationDate { get; set; } = string.Empty;

        public List<string> DescriptionDebt { get; set; } = default!;
    }

  
}
