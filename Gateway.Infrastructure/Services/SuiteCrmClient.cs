using System.Net.Http.Json;
using System.Text.Json;
using Gateway.Domain.Interfaces;
using Gateway.Domain.Models;
using Gateway.Infrastructure.Configuration;
using Gateway.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gateway.Infrastructure.Services;

public class SuiteCrmClient : ISuiteCrmClient
{
    private readonly HttpClient _httpClient;
    private readonly SuiteCrmOptions _options;
    private readonly ILogger<SuiteCrmClient> _logger;

    public SuiteCrmClient(
        HttpClient httpClient,
        IOptions<SuiteCrmOptions> options,
        ILogger<SuiteCrmClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TResponse?> ExecuteMethodAsync<TResponse>(
        string method,
        object restData,
        CancellationToken cancellationToken = default)
    {
        string endpoint = _options.FullEndpointUrl;
        string restDataJson = JsonSerializer.Serialize(restData);

        var formData = new Dictionary<string, string>
        {
            { "method", method },
            { "input_type", "JSON" },
            { "response_type", "JSON" },
            { "rest_data", restDataJson }
        };

        using var content = new FormUrlEncodedContent(formData);

        _logger.LogDebug("Executing SuiteCRM REST method '{Method}' to {Endpoint}", method, endpoint);

        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("SuiteCRM method '{Method}' raw response: {Response}", method, jsonResponse);

        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response for method '{Method}'. Content: {Content}", method, jsonResponse);
            throw;
        }
    }

    public async Task<SuiteCrmLoginResponse?> LoginAsync(
        string username,
        string passwordPlainOrMd5,
        string applicationName,
        CancellationToken cancellationToken = default)
    {
        string md5Password = Md5Helper.EnsureMd5(passwordPlainOrMd5);

        var loginPayload = new SuiteCrmLoginRequest
        {
            UserAuth = new SuiteCrmUserAuth
            {
                UserName = username,
                Password = md5Password,
                Version = "1"
            },
            ApplicationName = string.IsNullOrWhiteSpace(applicationName) ? _options.ApplicationName : applicationName,
            NameValueList = new List<NameValue>()
        };

        return await ExecuteMethodAsync<SuiteCrmLoginResponse>("login", loginPayload, cancellationToken);
    }

    public async Task<string?> GetUserIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var requestPayload = new SuiteCrmGetUserIdRequest { Session = sessionId };
        
        // SuiteCRM get_user_id returns a string (the user id GUID) or JSON string
        var jsonElement = await ExecuteMethodAsync<JsonElement>("get_user_id", requestPayload, cancellationToken);

        if (jsonElement.ValueKind == JsonValueKind.String)
        {
            return jsonElement.GetString();
        }

        if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("id", out var idProp))
        {
            return idProp.GetString();
        }

        return jsonElement.ToString();
    }

    public async Task<object?> GetEntriesAsync(string sessionId, string moduleName, string query = "", int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var requestPayload = new SuiteCrmGetEntryListRequest
        {
            Session = sessionId,
            ModuleName = moduleName,
            Query = query,
            MaxResults = maxResults,
            Offset = 0,
            Deleted = 0
        };

        return await ExecuteMethodAsync<object>("get_entry_list", requestPayload, cancellationToken);
    }

    public async Task<bool> LogoutAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var requestPayload = new SuiteCrmLogoutRequest { Session = sessionId };
        try
        {
            await ExecuteMethodAsync<object>("logout", requestPayload, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred during SuiteCRM logout for session '{SessionId}'", sessionId);
            return false;
        }
    }
}
