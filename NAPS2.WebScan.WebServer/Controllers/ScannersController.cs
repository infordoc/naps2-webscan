using Microsoft.AspNetCore.Mvc;
using NAPS2.WebScan.WebServer.Services;
using NAPS2.WebScan.WebServer.Models;

namespace NAPS2.WebScan.WebServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScannersController : ControllerBase
{
    private readonly LocalServiceClient _localServiceClient;
    private readonly ILogger<ScannersController> _logger;

    public ScannersController(LocalServiceClient localServiceClient, ILogger<ScannersController> logger)
    {
        _localServiceClient = localServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os scanners disponíveis com suas informações e URLs de capacidades
    /// </summary>
    /// <returns>Lista de scanners disponíveis</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScannerListDto>), 200)]
    public async Task<IActionResult> GetAllScanners()
    {
        var scanners = await _localServiceClient.GetAllScannersAsync();
        _logger.LogInformation("Listando {Count} scanners disponíveis", scanners.Count());
        return Ok(scanners);
    }

    /// <summary>
    /// Obtém informações detalhadas de um scanner específico
    /// </summary>
    /// <param name="id">ID do scanner</param>
    /// <returns>Informações do scanner</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScannerListDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetScanner(string id)
    {
        var scanner = await _localServiceClient.GetScannerAsync(id);
        
        if (scanner == null)
        {
            _logger.LogWarning("Scanner não encontrado: {ScannerId}", id);
            return NotFound(new { message = $"Scanner com ID '{id}' não encontrado" });
        }

        return Ok(scanner);
    }

    /// <summary>
    /// Retorna o total de scanners disponíveis
    /// </summary>
    /// <returns>Número de scanners</returns>
    [HttpGet("count")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetScannersCount()
    {
        var scanners = await _localServiceClient.GetAllScannersAsync();
        var count = scanners.Count();
        return Ok(new { count, timestamp = DateTime.UtcNow });
    }
}
