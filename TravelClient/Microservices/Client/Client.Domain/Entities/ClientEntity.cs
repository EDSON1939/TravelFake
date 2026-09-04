namespace Client.Domain.Entities;

/// <summary>
/// Proyección de la tabla TRAVELFAKE..CLIENTES con el país asociado.
/// Los nombres coinciden con los alias que devuelven
/// travelfake.GET_CLIENTE_ALL y travelfake.GET_CLIENTE_BY_ID.
/// </summary>
public class ClientEntity
{
    public long      CustomerId  { get; set; }
    public string    FirstName   { get; set; } = string.Empty;
    public string    LastName    { get; set; } = string.Empty;
    public string    Email       { get; set; } = string.Empty;
    public string    Phone       { get; set; } = string.Empty;
    public long      CountryId   { get; set; }
    public string    CountryName { get; set; } = string.Empty;
    public string    CountryCode { get; set; } = string.Empty;
    public bool      IsActive    { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
}
