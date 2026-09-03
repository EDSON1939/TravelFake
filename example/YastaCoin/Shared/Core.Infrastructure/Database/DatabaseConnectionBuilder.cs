using ConfigurationValidation;
using Microsoft.Data.SqlClient;

namespace Core.Infrastructure.Database
{
    public class DatabaseConnectionBuilder : IValidatableConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;

        public IEnumerable<ConfigurationValidationItem> Validate()
        {
            var validationItems = new List<ConfigurationValidationItem>();
            try
            {
                var connectionObject = new SqlConnectionStringBuilder(ConnectionString);
            }
            catch (Exception exception)
            {
                validationItems.Add(new ConfigurationValidationItem("Database", nameof(ConnectionString), ConnectionString, exception.Message));
            }

            return validationItems;
        }
    }
}
