namespace Core.Domain.Models.ServicePayment.Responses
{
    public class PaymentResponse
    {
        public long TransactionId { get; set; }

        public string ConciliationId { get; set; } = string.Empty;

        public string OperationNumber { get; set; } = string.Empty;

        public string DebtId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;
    }
}
