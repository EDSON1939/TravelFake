namespace Auth.Application.Common;

public record LoginResponse(
    string   AccessToken,
    DateTime ExpiresAt,
    int      ExpiresIn,
    long     UserId,
    long?    ClientId,
    string   Username,
    string   FullName,
    string   Role);
