using System.Collections.Concurrent;
using NAPS2.Images.ImageSharp;
using NAPS2.Scan;
using NAPS2.WebScan.LocalService.Models;

namespace NAPS2.WebScan.LocalService.Services;

public class ScannerService
{
    private readonly ILogger<ScannerService> _logger;
    private readonly ScanningContext _scanningContext;
    private string? _defaultScannerId;
    private readonly ConcurrentDictionary<string, ScanDevice> _cachedScanners = new();

    public ScannerService(ILogger<ScannerService> logger)
    {
        _logger = logger;
        _scanningContext = new ScanningContext(new ImageSharpImageContext());
    }

    public ScanningContext ScanningContext => _scanningContext;

    public async Task<List<ScannerInfo>> GetScannersAsync()
    {
        try
        {
            _logger.LogInformation("Getting list of TWAIN scanners...");
            
            var controller = new ScanController(_scanningContext);
            var devices = await controller.GetDeviceList(Driver.Twain);
            
            _cachedScanners.Clear();
            var scanners = new List<ScannerInfo>();
            
            foreach (var device in devices)
            {
                var scannerId = $"{device.Driver}_{device.ID}";
                _cachedScanners[scannerId] = device;
                
                // Note: Capabilities are currently hardcoded as NAPS2.SDK does not provide
                // a straightforward way to query device capabilities without initiating a scan.
                // Actual capabilities may vary by device.
                var scannerInfo = new ScannerInfo
                {
                    Id = scannerId,
                    Name = device.Name,
                    Driver = device.Driver.ToString(),
                    IsDefault = scannerId == _defaultScannerId,
                    Capabilities = new ScannerCapabilities
                    {
                        SupportedResolutions = new List<int> { 100, 150, 200, 300, 600, 1200 },
                        SupportedColorModes = new List<string> { "Color", "Grayscale", "BlackAndWhite" },
                        SupportedPageSizes = new List<string> { "A4", "Letter", "Legal" },
                        HasADF = true,
                        HasFlatbed = true
                    }
                };
                
                scanners.Add(scannerInfo);
            }
            
            _logger.LogInformation("Found {Count} TWAIN scanners", scanners.Count);
            return scanners;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scanner list");
            throw;
        }
    }

    public ScannerInfo? GetDefaultScanner()
    {
        if (_defaultScannerId == null)
        {
            return null;
        }

        if (_cachedScanners.TryGetValue(_defaultScannerId, out var device))
        {
            return new ScannerInfo
            {
                Id = _defaultScannerId,
                Name = device.Name,
                Driver = device.Driver.ToString(),
                IsDefault = true
            };
        }

        return null;
    }

    public bool SetDefaultScanner(string scannerId)
    {
        if (_cachedScanners.ContainsKey(scannerId))
        {
            _defaultScannerId = scannerId;
            _logger.LogInformation("Default scanner set to: {ScannerId}", scannerId);
            return true;
        }
        
        _logger.LogWarning("Scanner not found: {ScannerId}", scannerId);
        return false;
    }

    public ScanDevice? GetScanDevice(string? scannerId)
    {
        if (scannerId == null)
        {
            scannerId = _defaultScannerId;
        }

        if (scannerId != null && _cachedScanners.TryGetValue(scannerId, out var device))
        {
            return device;
        }

        return null;
    }
}
