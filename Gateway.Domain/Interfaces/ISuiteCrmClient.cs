using Gateway.Domain.Models;

namespace Gateway.Domain.Interfaces;

public interface ISuiteCrmClient
{
    Task<TResponse?> ExecuteMethodAsync<TResponse>(string method, object restData, CancellationToken cancellationToken = default);
    Task<SuiteCrmLoginResponse?> LoginAsync(string username, string passwordPlainOrMd5, string applicationName, CancellationToken cancellationToken = default);
    Task<string?> GetUserIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<object?> GetEntriesAsync(string sessionId, string moduleName, string query = "", int maxResults = 10, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string sessionId, CancellationToken cancellationToken = default);
}
