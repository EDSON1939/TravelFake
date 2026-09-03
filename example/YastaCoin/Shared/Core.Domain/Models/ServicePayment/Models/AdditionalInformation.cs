namespace Core.Domain.Models.ServicePayment.Models
{
    public class AdditionalInformation
    {
        public string CategoryCode { get; set; } = string.Empty;

        public string CompanyCode { get; set; } = string.Empty;

        public string ServiceCode { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public string Information { get; set; } = string.Empty;
    }
}
