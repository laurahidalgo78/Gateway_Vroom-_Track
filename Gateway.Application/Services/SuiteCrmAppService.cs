using Gateway.Domain.Interfaces;
using Gateway.Domain.Models;

namespace Gateway.Application.Services;

public class SuiteCrmAppService
{
    private readonly ISuiteCrmValidationService _validationService;
    private readonly ISuiteCrmClient _client;

    public SuiteCrmAppService(
        ISuiteCrmValidationService validationService,
        ISuiteCrmClient client)
    {
        _validationService = validationService;
        _client = client;
    }

    public async Task<SuiteCrmConnectionResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _validationService.ValidateConnectionAsync(cancellationToken);
    }

    public async Task<TResponse?> ExecuteMethodAsync<TResponse>(string method, object restData, CancellationToken cancellationToken = default)
    {
        return await _client.ExecuteMethodAsync<TResponse>(method, restData, cancellationToken);
    }

    public async Task<object?> FetchModuleEntriesAsync(string moduleName, string query = "", int maxResults = 10, CancellationToken cancellationToken = default)
    {
        // Step 1: Login
        var loginResponse = await _client.LoginAsync("admin", "123Alejandro.", "VroomService Gateway", cancellationToken);
        if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.Id))
        {
            throw new InvalidOperationException("No se pudo iniciar sesión en SuiteCRM. Verifica las credenciales.");
        }

        string sessionId = loginResponse.Id;
        try
        {
            // Step 2: Fetch module entries
            return await _client.GetEntriesAsync(sessionId, moduleName, query, maxResults, cancellationToken);
        }
        finally
        {
            // Step 3: Logout
            await _client.LogoutAsync(sessionId, cancellationToken);
        }
    }
}
