using NAPS2.Escl.Server;
using NAPS2.Scan;
using NAPS2.Remoting.Server;
using NAPS2.WebScan.LocalService.Models;

namespace NAPS2.WebScan.LocalService.Services;

public class ScannerManagerService
{
    private readonly ScannerRegistryService _scannerRegistry;
    private readonly ILogger<ScannerManagerService> _logger;
    private ScanServer? _scanServer;
    private ScanDevice? _currentDevice;

    public ScannerManagerService(ScannerRegistryService scannerRegistry, ILogger<ScannerManagerService> logger)
    {
        _scannerRegistry = scannerRegistry;
        _logger = logger;
    }

    public void SetScanServer(ScanServer scanServer)
    {
        _scanServer = scanServer;
    }

    public void SetCurrentDevice(ScanDevice device)
    {
        _currentDevice = device;
        _logger.LogInformation("Dispositivo atual definido: {DeviceName}", device.Name);
    }

    public async Task<bool> SwitchToDevice(string deviceId)
    {
        if (_scanServer == null)
        {
            _logger.LogWarning("Servidor ESCL não está inicializado");
            return false;
        }

        var scanner = _scannerRegistry.GetScanner(deviceId);
        if (scanner == null)
        {
            _logger.LogWarning("Scanner não encontrado: {DeviceId}", deviceId);
            return false;
        }

        // Se já é o dispositivo atual, não faz nada
        if (_currentDevice?.ID == scanner.Device.ID)
        {
            _logger.LogInformation("Scanner {DeviceName} já está selecionado", scanner.Name);
            return true;
        }

        // NOTA: No NAPS2, trocar de scanner em runtime requer reiniciar o serviço
        // Por enquanto, apenas atualizamos a referência do dispositivo atual
        // O servidor ESCL continuará servindo todos os dispositivos registrados
        _currentDevice = scanner.Device;
        _logger.LogInformation("Scanner atual alterado para: {DeviceName}", scanner.Name);
        _logger.LogInformation("Nota: O servidor ESCL serve todos os scanners na porta 9880");
        
        return true;
    }

    public ScanDevice? GetCurrentDevice() => _currentDevice;
}
