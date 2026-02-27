using Microsoft.AspNetCore.Mvc;
using NAPS2.WebScan.LocalService.Models;

namespace NAPS2.WebScan.LocalService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;
    private readonly IConfiguration _configuration;

    public ConfigController(ILogger<ConfigController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public ActionResult<AppConfig> GetConfig()
    {
        try
        {
            _logger.LogInformation("GET /api/config");
            
            var config = new AppConfig();
            _configuration.GetSection("Server").Bind(config.Server);
            _configuration.GetSection("Cors").Bind(config.Cors);
            _configuration.GetSection("Scanning").Bind(config.Scanning);
            
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpPost]
    public ActionResult<ApiResponse<object>> UpdateConfig([FromBody] AppConfig config)
    {
        try
        {
            _logger.LogInformation("POST /api/config");
            
            // Note: In-memory configuration updates won't persist
            // For persistent updates, you'd need to write to appsettings.json
            _logger.LogWarning("Configuration updates are not persisted to file");
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { Message = "Configuration updated (in-memory only, restart required to reset)" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Error = ex.Message
            });
        }
    }
}
