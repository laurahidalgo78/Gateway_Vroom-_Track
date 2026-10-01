using System.Text.Json.Serialization;

namespace Gateway.Domain.Models;

/// <summary>
/// Domain models and DTOs representing the SuiteCRM REST v4_1 protocol payloads.
/// </summary>

public class SuiteCrmUserAuth
{
    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty; // MD5 hash string

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1";
}

public class SuiteCrmLoginRequest
{
    [JsonPropertyName("user_auth")]
    public SuiteCrmUserAuth UserAuth { get; set; } = new();

    [JsonPropertyName("application_name")]
    public string ApplicationName { get; set; } = "Gateway";

    [JsonPropertyName("name_value_list")]
    public List<NameValue> NameValueList { get; set; } = new();
}

public class SuiteCrmLoginResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("module_name")]
    public string? ModuleName { get; set; }

    [JsonPropertyName("name_value_list")]
    public Dictionary<string, NameValueDetail>? NameValueList { get; set; }

    [JsonPropertyName("number")]
    public int? Number { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class SuiteCrmGetUserIdRequest
{
    [JsonPropertyName("session")]
    public string Session { get; set; } = string.Empty;
}

public class SuiteCrmLogoutRequest
{
    [JsonPropertyName("session")]
    public string Session { get; set; } = string.Empty;
}

public class NameValue
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class NameValueDetail
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public class SuiteCrmExecuteRequest
{
    public string Method { get; set; } = string.Empty;
    public object RestData { get; set; } = new();
}

public class SuiteCrmGetEntryListRequest
{
    [JsonPropertyName("session")]
    public string Session { get; set; } = string.Empty;

    [JsonPropertyName("module_name")]
    public string ModuleName { get; set; } = string.Empty;

    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("order_by")]
    public string OrderBy { get; set; } = string.Empty;

    [JsonPropertyName("offset")]
    public int Offset { get; set; } = 0;

    [JsonPropertyName("select_fields")]
    public List<string> SelectFields { get; set; } = new();

    [JsonPropertyName("link_name_to_fields_array")]
    public List<object> LinkNameToFieldsArray { get; set; } = new();

    [JsonPropertyName("max_results")]
    public int MaxResults { get; set; } = 20;

    [JsonPropertyName("deleted")]
    public int Deleted { get; set; } = 0;
}

public class SuiteCrmFetchModuleDataRequest
{
    public string ModuleName { get; set; } = "Contacts";
    public string Query { get; set; } = "";
    public int MaxResults { get; set; } = 10;
}

public class SuiteCrmConnectionResult
{
    public bool IsConnected { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public double LatencyMs { get; set; }
    public int HttpStatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object?> Diagnostics { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
