namespace Country.Domain.Entities;

/// <summary>
/// Proyección de la tabla TRAVELFAKE..PAIS.
/// Los nombres coinciden con los alias que devuelven dbo.GET_PAIS_ALL y dbo.GET_PAIS_BY_ID.
/// </summary>
public class CountryEntity
{
    public long      CountryId { get; set; }
    public string    Name      { get; set; } = string.Empty;
    public string    Code      { get; set; } = string.Empty;
    public bool      IsActive  { get; set; }
    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
