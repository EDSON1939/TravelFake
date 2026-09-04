using Core.Domain.Enum;

namespace Core.Domain.Models.ServicePayment.Models
{
    public class BillingCriteria
    {
        public Billing Code { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public List<CriteriaItem> Items { get; set; } = default!;

        public string Type { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool Required { get; set; }

    }
    public class CriteriaItem
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
