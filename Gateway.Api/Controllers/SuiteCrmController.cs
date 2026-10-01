using Gateway.Application.Services;
using Gateway.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SuiteCrmController : ControllerBase
{
    private readonly SuiteCrmAppService _suiteCrmAppService;
    private readonly ILogger<SuiteCrmController> _logger;

    public SuiteCrmController(
        SuiteCrmAppService suiteCrmAppService,
        ILogger<SuiteCrmController> logger)
    {
        _suiteCrmAppService = suiteCrmAppService;
        _logger = logger;
    }

    /// <summary>
    /// Validates full connection and consumption against SuiteCRM REST v4_1 Gateway.
    /// Performs Login -> Get User ID -> Session Logout cycle.
    /// </summary>
    [HttpPost("validate-connection")]
    [HttpGet("validate-connection")]
    [ProducesResponseType(typeof(SuiteCrmConnectionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuiteCrmConnectionResult), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ValidateConnection(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to validate SuiteCRM Gateway connection.");
        var result = await _suiteCrmAppService.ValidateConnectionAsync(cancellationToken);

        if (result.IsConnected)
        {
            return Ok(result);
        }

        return StatusCode(result.HttpStatusCode > 0 ? result.HttpStatusCode : 503, result);
    }

    /// <summary>
    /// Executes a custom SuiteCRM REST v4_1 method dynamically for testing and consumption.
    /// </summary>
    [HttpPost("execute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteMethod(
        [FromBody] SuiteCrmExecuteRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Method))
        {
            return BadRequest(new { error = "El nombre del método ('method') es obligatorio." });
        }

        _logger.LogInformation("Executing custom SuiteCRM REST method '{Method}'", request.Method);
        
        var response = await _suiteCrmAppService.ExecuteMethodAsync<object>(request.Method, request.RestData, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Fetches existing entries/records from a specified SuiteCRM module (e.g., Contacts, vr_Vehiculos, vr_Ordenes_Servicio, vr_Centros_Servicio, AOS_Products).
    /// </summary>
    [HttpGet("modules/{moduleName}")]
    [HttpPost("query")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> FetchModuleData(
        string moduleName,
        [FromQuery] string? query,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            return BadRequest(new { error = "El nombre del módulo ('moduleName') es obligatorio. Ejemplo: Contacts, vr_Vehiculos, vr_Ordenes_Servicio." });
        }

        _logger.LogInformation("Fetching entries for SuiteCRM module '{ModuleName}'", moduleName);

        try
        {
            var data = await _suiteCrmAppService.FetchModuleEntriesAsync(moduleName, query ?? "", maxResults, cancellationToken);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching module data for '{ModuleName}'", moduleName);
            return StatusCode(500, new { error = ex.Message, moduleName });
        }
    }
}
