using Microsoft.AspNetCore.Mvc;
using NAPS2.WebScan.LocalService.Models;
using NAPS2.WebScan.LocalService.Services;

namespace NAPS2.WebScan.LocalService.Controllers;

[ApiController]
[Route("api")]
public class ScanController : ControllerBase
{
    private readonly ILogger<ScanController> _logger;
    private readonly ScanningService _scanningService;

    public ScanController(ILogger<ScanController> logger, ScanningService scanningService)
    {
        _logger = logger;
        _scanningService = scanningService;
    }

    [HttpPost("scan")]
    public async Task<ActionResult<ScanResponse>> Scan([FromBody] ScanRequest request)
    {
        try
        {
            _logger.LogInformation("POST /api/scan");
            
            var response = await _scanningService.ScanAsync(request);
            
            return response.Success ? Ok(response) : BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scan operation");
            return StatusCode(500, new ScanResponse
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpGet("health")]
    public ActionResult<object> HealthCheck()
    {
        _logger.LogInformation("GET /api/health");
        
        return Ok(new
        {
            Status = "Healthy",
            Service = "NAPS2 WebScan Server",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow
        });
    }
}
