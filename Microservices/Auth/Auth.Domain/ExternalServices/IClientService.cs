namespace Auth.Domain.ExternalServices;

public interface IClientService
{
    Task<ClientInfo?> GetByDocument(string document, CancellationToken ct = default);
}

public record ClientInfo(long ClientId, string Document, string FullName, bool IsActive);
