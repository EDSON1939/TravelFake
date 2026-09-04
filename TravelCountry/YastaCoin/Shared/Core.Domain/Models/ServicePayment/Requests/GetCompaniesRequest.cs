namespace Core.Domain.Models.ServicePayment.Requests
{
    public class GetCompaniesRequest
    {
        public string InstitutionCode { get; set; } = string.Empty;

        public string ChannelCode { get; set; } = string.Empty;
    }
}
