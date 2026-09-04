namespace Client.Domain.Entities;

/// <summary>
/// País obtenido del microservicio TravelCountry.
/// No se persiste en TRAVELFAKE..CLIENTES: solo viaja para validar y enriquecer al cliente.
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
