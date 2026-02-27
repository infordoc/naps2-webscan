using Microsoft.AspNetCore.Mvc;
using NAPS2.WebScan.LocalService.Models;
using NAPS2.WebScan.LocalService.Services;

namespace NAPS2.WebScan.LocalService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScannersController : ControllerBase
{
    private readonly ILogger<ScannersController> _logger;
    private readonly ScannerService _scannerService;

    public ScannersController(ILogger<ScannersController> logger, ScannerService scannerService)
    {
        _logger = logger;
        _scannerService = scannerService;
    }

    [HttpGet]
    public async Task<ActionResult<ScannersResponse>> GetScanners()
    {
        try
        {
            _logger.LogInformation("GET /api/scanners");
            
            var scanners = await _scannerService.GetScannersAsync();
            return Ok(new ScannersResponse { Scanners = scanners });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scanners");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpGet("default")]
    public ActionResult<ApiResponse<ScannerInfo>> GetDefaultScanner()
    {
        try
        {
            _logger.LogInformation("GET /api/scanners/default");
            
            var defaultScanner = _scannerService.GetDefaultScanner();
            
            if (defaultScanner == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = "No default scanner set"
                });
            }
            
            return Ok(new ApiResponse<ScannerInfo>
            {
                Success = true,
                Data = defaultScanner
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default scanner");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpPost("default")]
    public ActionResult<ApiResponse<object>> SetDefaultScanner([FromBody] SetDefaultScannerRequest request)
    {
        try
        {
            _logger.LogInformation("POST /api/scanners/default");
            
            if (string.IsNullOrEmpty(request.ScannerId))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = "ScannerId is required"
                });
            }
            
            var success = _scannerService.SetDefaultScanner(request.ScannerId);
            
            if (success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { ScannerId = request.ScannerId }
                });
            }
            else
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = $"Scanner not found: {request.ScannerId}"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default scanner");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Error = ex.Message
            });
        }
    }
}
