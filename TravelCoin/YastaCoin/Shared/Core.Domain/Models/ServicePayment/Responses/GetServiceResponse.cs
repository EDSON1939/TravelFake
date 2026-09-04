namespace Core.Domain.Models.ServicePayment.Responses
{
    public class GetServiceResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public string BillingType { get; set; } = string.Empty;

        public string SearchPaymentValidation { get; set; } = string.Empty;

        public string PaymentItemOrder { get; set; } = string.Empty;

        public bool ReturnVoucher { get; set; }

        public Availability? Availability { get; set; }

        public List<SearchCriteria> SearchCriteria { get; set; } = default!;
    }

    public class Availability(bool state, string hours)
    {
        public bool State { get; set; } = state;

        public string Hours { get; set; } = hours;
    }

    public class SearchCriteria(string name, string description, string code, int order, string controlType, bool required, bool visible, List<Item> items, string? entryType, string? entryValidation, bool enabled = true, string value = "")
    {
        public string Name { get; set; } = name;

        public string Code { get; set; } = code;

        public int Order { get; set; } = order;

        public string ControlType { get; set; } = controlType;

        public string? EntryType { get; set; } = entryType;

        public string? EntryValidation { get; set; } = entryValidation;

        public string Description { get; set; } = description;

        public bool Required { get; set; } = required;

        public bool Visible { get; set; } = visible;

        public bool Enabled { get; set; } = enabled;

        public List<Item> Items { get; set; } = items;

        public string Value { get; set; } = value;
    }

    public class Item
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
