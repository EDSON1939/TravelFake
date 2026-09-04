namespace Client.Application.Common;

/// <summary>
/// País expuesto por este microservicio tal como lo devuelve TravelCountry.
/// Sirve para poblar el selector de país antes de dar de alta un cliente.
/// </summary>
public record CountryResponse(
    long   CountryId,
    string Name,
    string Code,
    bool   IsActive);
