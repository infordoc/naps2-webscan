using NAPS2.WebScan.WebServer.Models;

namespace NAPS2.WebScan.WebServer.Services;

public class LocalServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocalServiceClient> _logger;
    private readonly string _localServiceUrl;

    public LocalServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<LocalServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _localServiceUrl = configuration["LocalService:Url"] ?? "http://localhost:5000";
    }

    public async Task<IEnumerable<ScannerListDto>> GetAllScannersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_localServiceUrl}/api/scanners");
            response.EnsureSuccessStatusCode();
            
            var scanners = await response.Content.ReadFromJsonAsync<IEnumerable<ScannerListDto>>();
            _logger.LogInformation("Obtidos {Count} scanners do LocalService", scanners?.Count() ?? 0);
            
            return scanners ?? Enumerable.Empty<ScannerListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter scanners do LocalService em {Url}", _localServiceUrl);
            return Enumerable.Empty<ScannerListDto>();
        }
    }

    public async Task<ScannerListDto?> GetScannerAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_localServiceUrl}/api/scanners/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            
            var scanner = await response.Content.ReadFromJsonAsync<ScannerListDto>();
            return scanner;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter scanner {Id} do LocalService", id);
            return null;
        }
    }

    public async Task<bool> SelectScannerAsync(string id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_localServiceUrl}/api/scanners/{id}/select", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao selecionar scanner {Id}", id);
            return false;
        }
    }
}
