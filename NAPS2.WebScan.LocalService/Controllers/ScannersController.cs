using Microsoft.AspNetCore.Mvc;
using NAPS2.WebScan.LocalService.Services;
using NAPS2.WebScan.LocalService.Models;

namespace NAPS2.WebScan.LocalService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScannersController : ControllerBase
{
    private readonly ScannerRegistryService _scannerRegistry;
    private readonly ScannerManagerService _scannerManager;
    private readonly ILogger<ScannersController> _logger;

    public ScannersController(
        ScannerRegistryService scannerRegistry, 
        ScannerManagerService scannerManager,
        ILogger<ScannersController> logger)
    {
        _scannerRegistry = scannerRegistry;
        _scannerManager = scannerManager;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os scanners disponíveis
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScannerListDto>), 200)]
    public IActionResult GetAllScanners()
    {
        var scanners = _scannerRegistry.GetAllScanners()
            .Select(s => new ScannerListDto
            {
                Id = s.Id,
                Name = s.Name,
                Port = s.Port,
                RegisteredAt = s.RegisteredAt
            });

        _logger.LogInformation("Listando {Count} scanners disponíveis", scanners.Count());
        return Ok(scanners);
    }

    /// <summary>
    /// Obtém informações de um scanner específico
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScannerListDto), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetScanner(string id)
    {
        var scanner = _scannerRegistry.GetScanner(id);
        
        if (scanner == null)
        {
            return NotFound(new { message = $"Scanner '{id}' não encontrado" });
        }

        var dto = new ScannerListDto
        {
            Id = scanner.Id,
            Name = scanner.Name,
            Port = scanner.Port,
            RegisteredAt = scanner.RegisteredAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Seleciona qual scanner será usado para o próximo scan
    /// </summary>
    [HttpPost("{id}/select")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SelectScanner(string id)
    {
        var scanner = _scannerRegistry.GetScanner(id);
        if (scanner == null)
        {
            return NotFound(new { message = $"Scanner '{id}' não encontrado" });
        }

        var success = await _scannerManager.SwitchToDevice(id);
        
        if (!success)
        {
            return StatusCode(500, new { message = "Erro ao selecionar o scanner" });
        }

        return Ok(new 
        { 
            message = $"Scanner '{scanner.Name}' selecionado com sucesso",
            scannerId = id,
            scannerName = scanner.Name,
            port = scanner.Port,
            capabilitiesUrl = $"http://localhost:{scanner.Port}/eSCL/ScannerCapabilities"
        });
    }

    /// <summary>
    /// Obtém o scanner atualmente selecionado
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ScannerListDto), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetCurrentScanner()
    {
        var currentDevice = _scannerManager.GetCurrentDevice();
        
        if (currentDevice == null)
        {
            return NotFound(new { message = "Nenhum scanner selecionado" });
        }

        var scanner = _scannerRegistry.GetScanner(currentDevice.ID);
        if (scanner == null)
        {
            return NotFound(new { message = "Scanner atual não encontrado no registro" });
        }

        var dto = new ScannerListDto
        {
            Id = scanner.Id,
            Name = scanner.Name,
            Port = scanner.Port,
            RegisteredAt = scanner.RegisteredAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Retorna o total de scanners disponíveis
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetScannersCount()
    {
        var count = _scannerRegistry.Count;
        return Ok(new { count, timestamp = DateTime.UtcNow });
    }
}
