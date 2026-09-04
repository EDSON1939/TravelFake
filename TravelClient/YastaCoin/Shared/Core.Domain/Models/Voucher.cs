using System.Text.Json.Serialization;

namespace Core.Domain.Models
{
    public class Voucher
    {
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("subtitle")]
        public string SubTitle { get; set; } = string.Empty;

        [JsonPropertyName("dateTime")]
        public string DateTime { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("vouchers")]
        public List<VoucherItem> Vouchers { get; set; } = [];
    }

    public class VoucherItem
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
