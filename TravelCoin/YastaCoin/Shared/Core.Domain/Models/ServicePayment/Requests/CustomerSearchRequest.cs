using Core.Domain.Models.ServicePayment.Models;

namespace Core.Domain.Models.ServicePayment.Requests
{
    public class CustomerSearchRequest
    {
        public string ChannelCode { get; set; } = string.Empty;

        public string CompanyCode { get; set; } = string.Empty;

        public string ServiceCode { get; set; } = string.Empty;

        public string Terminal { get; set; } = string.Empty;

        public string Operator { get; set; } = string.Empty;

        public AdditionalInformation AdditionalInformation { get; set; } = default!;

        public List<Criteria> SearchCriteria { get; set; } = default!;

        public bool AdditionalSearchCriteria { get; set; }

        public List<BillingCriteria> BillingCriteria { get; set; } = default!;
    }

    public class Criteria
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    public class UnibancaInformation
    {
        public int CriteriaCode { get; set; }

        public string Operator { get; set; } = string.Empty;
    }
  
}
