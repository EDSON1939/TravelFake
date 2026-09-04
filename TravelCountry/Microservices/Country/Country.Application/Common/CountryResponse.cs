namespace Country.Application.Common;

public record CountryResponse(
    long      CountryId,
    string    Name,
    string    Code,
    bool      IsActive,
    DateTime  CreatedAt,
    DateTime? UpdatedAt);
