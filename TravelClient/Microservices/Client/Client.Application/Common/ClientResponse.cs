namespace Client.Application.Common;

public record ClientResponse(
    long      CustomerId,
    string    FirstName,
    string    LastName,
    string    Email,
    string    Phone,
    long      CountryId,
    string    CountryName,
    string    CountryCode,
    bool      IsActive,
    DateTime  CreatedAt,
    DateTime? UpdatedAt);
