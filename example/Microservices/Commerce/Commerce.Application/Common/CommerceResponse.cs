namespace Commerce.Application.Common;

public record CommerceResponse(
    long                     CommerceId,
    string                   Name,
    string                   Nit,
    bool                     IsActive,
    DateTime                 CreatedAt,
    DateTime?                UpdatedAt);