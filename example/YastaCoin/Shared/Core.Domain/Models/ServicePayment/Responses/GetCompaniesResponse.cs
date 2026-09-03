namespace Core.Domain.Models.ServicePayment.Responses
{
    public class GetCompaniesResponse(string code, string name, string image, string description)
    {
        public string Code { get; set; } = code;

        public string Name { get; set; } = name;

        public string Image { get; set; } = image;

        public string Description { get; set; } = description;

        public List<Company> Companies { get; set; } = default!;
    }

    public class Company(string code, string name, string image, string state)
    {
        public string Code { get; set; } = code;

        public string Name { get; set; } = name;

        public string Image { get; set; } = image;

        public string State { get; set; } = state;
    }
}
