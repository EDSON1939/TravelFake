namespace Commerce.Application.Common;

public record CommerceResponse(
    long                     CommerceId,
    string                   Name,
    string                   Nit,
    long?                    CuentaId,
    bool                     IsActive,
    DateTime                 CreatedAt,
    DateTime?                UpdatedAt);
