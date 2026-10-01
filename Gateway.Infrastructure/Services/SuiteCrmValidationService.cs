using System.Diagnostics;
using Gateway.Domain.Interfaces;
using Gateway.Domain.Models;
using Gateway.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gateway.Infrastructure.Services;

public class SuiteCrmValidationService : ISuiteCrmValidationService
{
    private readonly ISuiteCrmClient _client;
    private readonly SuiteCrmOptions _options;
    private readonly ILogger<SuiteCrmValidationService> _logger;

    public SuiteCrmValidationService(
        ISuiteCrmClient client,
        IOptions<SuiteCrmOptions> options,
        ILogger<SuiteCrmValidationService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SuiteCrmConnectionResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var result = new SuiteCrmConnectionResult
        {
            Diagnostics = new Dictionary<string, object?>
            {
                { "TargetEndpoint", _options.FullEndpointUrl },
                { "ConfiguredUser", _options.Username },
                { "ApplicationName", _options.ApplicationName }
            }
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Initiating SuiteCRM API Gateway connection validation against {Endpoint}...", _options.FullEndpointUrl);

            // Step 1: Execute Login
            var loginResponse = await _client.LoginAsync(
                _options.Username,
                !string.IsNullOrWhiteSpace(_options.PasswordMd5) ? _options.PasswordMd5 : _options.Password,
                _options.ApplicationName,
                cancellationToken
            );

            if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.Id))
            {
                stopwatch.Stop();
                result.IsConnected = false;
                result.Status = "LOGIN_FAILED";
                result.HttpStatusCode = 200; // API returned HTTP 200 but error in JSON payload
                result.Message = loginResponse?.Description ?? "SuiteCRM returned empty session ID upon login attempt.";
                result.LatencyMs = stopwatch.ElapsedMilliseconds;
                
                if (loginResponse != null)
                {
                    result.Diagnostics["ErrorCode"] = loginResponse.Number;
                    result.Diagnostics["ErrorName"] = loginResponse.Name;
                    result.Diagnostics["ErrorDescription"] = loginResponse.Description;
                }

                _logger.LogWarning("SuiteCRM connection validation failed at Login step: {Message}", result.Message);
                return result;
            }

            string sessionId = loginResponse.Id;
            result.SessionId = sessionId;
            result.Diagnostics["SessionIdObtained"] = true;

            // Step 2: Validate Session and fetch User ID
            string? userId = await _client.GetUserIdAsync(sessionId, cancellationToken);
            result.UserId = userId;
            result.Diagnostics["UserIdRetrieved"] = userId;

            // Step 3: Logout clean-up
            bool loggedOut = await _client.LogoutAsync(sessionId, cancellationToken);
            result.Diagnostics["LoggedOutSuccessfully"] = loggedOut;

            stopwatch.Stop();

            result.IsConnected = true;
            result.Status = "CONNECTED_AND_VERIFIED";
            result.HttpStatusCode = 200;
            result.LatencyMs = stopwatch.ElapsedMilliseconds;
            result.Message = $"Conexión establecida con éxito con SuiteCRM REST Gateway. Session ID validado y User ID '{userId}' obtenido correctamente.";

            _logger.LogInformation("SuiteCRM connection validation SUCCEEDED in {LatencyMs} ms. User ID: {UserId}", result.LatencyMs, userId);
            return result;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            result.IsConnected = false;
            result.Status = "NETWORK_ERROR";
            result.HttpStatusCode = (int)(ex.StatusCode ?? System.Net.HttpStatusCode.ServiceUnavailable);
            result.LatencyMs = stopwatch.ElapsedMilliseconds;
            result.Message = $"Error de red o conexión al intentar consumir SuiteCRM Gateway en '{_options.FullEndpointUrl}': {ex.Message}";
            result.Diagnostics["ExceptionType"] = ex.GetType().Name;
            result.Diagnostics["HttpRequestError"] = ex.HttpRequestError.ToString();

            _logger.LogError(ex, "Network failure connecting to SuiteCRM Gateway at {Endpoint}", _options.FullEndpointUrl);
            return result;
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            result.IsConnected = false;
            result.Status = "TIMEOUT";
            result.HttpStatusCode = 408;
            result.LatencyMs = stopwatch.ElapsedMilliseconds;
            result.Message = $"Tiempo de espera agotado ({_options.TimeoutSeconds}s) al intentar conectar con SuiteCRM Gateway en '{_options.FullEndpointUrl}'.";
            result.Diagnostics["ExceptionType"] = ex.GetType().Name;

            _logger.LogError(ex, "Timeout contacting SuiteCRM Gateway");
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.IsConnected = false;
            result.Status = "UNHANDLED_ERROR";
            result.HttpStatusCode = 500;
            result.LatencyMs = stopwatch.ElapsedMilliseconds;
            result.Message = $"Error inesperado durante la validación de consumo con SuiteCRM: {ex.Message}";
            result.Diagnostics["ExceptionType"] = ex.GetType().Name;
            result.Diagnostics["StackTrace"] = ex.StackTrace;

            _logger.LogError(ex, "Unhandled error during SuiteCRM connection validation");
            return result;
        }
    }
}
