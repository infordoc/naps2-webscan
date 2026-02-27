using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Images.ImageSharp;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Threading;
using NAPS2.WebScan.LocalService.Models;
using NAPS2.WebScan.LocalService.Services;

namespace NAPS2.WebScan.LocalService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ScannerRegistryService _scannerRegistry;
    private readonly ScannerManagerService _scannerManager;

    public Worker(ILogger<Worker> logger, ScannerRegistryService scannerRegistry, ScannerManagerService scannerManager)
    {
        _logger = logger;
        _scannerRegistry = scannerRegistry;
        _scannerManager = scannerManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Iniciando Worker...");
            
            // Configurar contexto de scanning para TWAIN 64-bit
            var imageContext = new ImageSharpImageContext();
            using var scanningContext = new ScanningContext(imageContext);
            
            // Definir worker de 32 bits para TWAIN (mais estável)
            // Mesmo em processo 64-bit, isso cria um worker 32-bit para comunicar com drivers TWAIN
            scanningContext.SetUpWin32Worker();
            
            var controller = new ScanController(scanningContext);

            using var scanServer = new ScanServer(scanningContext, new EsclServer
            {
                // This line is required for scanning from a browser to work
                SecurityPolicy = EsclSecurityPolicy.ServerAllowAnyOrigin
            });

            // Validar arquitetura do processo
            _logger.LogInformation("Arquitetura do processo: {Arch}", 
                Environment.Is64BitProcess ? "64-bit" : "32-bit");
            _logger.LogInformation("Worker TWAIN 32-bit configurado para compatibilidade máxima");

            var scanOptions = new ScanOptions
            {
                Driver = Driver.Twain
            };

            _logger.LogInformation("Buscando dispositivos TWAIN...");
            var allDevices = await controller.GetDeviceList(scanOptions);
            
            // Filtrar apenas dispositivos TWAIN (excluir WIA)
            var devices = allDevices.Where(d => d.Driver == Driver.Twain).ToList();
            
            _logger.LogInformation("Encontrados {TotalCount} dispositivos totais, {TwainCount} TWAIN", 
                allDevices.Count(), devices.Count);

            if (!devices.Any())
            {
                _logger.LogWarning("Nenhum dispositivo TWAIN encontrado. Servidor não será iniciado.");
                if (allDevices.Any())
                {
                    var excludedDrivers = allDevices
                        .Where(d => d.Driver != Driver.Twain)
                        .Select(d => $"{d.Name} ({d.Driver})")
                        .ToList();
                    _logger.LogInformation("Dispositivos excluídos (não-TWAIN): {Excluded}", 
                        string.Join(", ", excludedDrivers));
                }
                return;
            }

            // Registrar todos os dispositivos TWAIN no serviço de registro
            int portBase = 9880;
            int portIndex = 0;
            
            foreach (var device in devices)
            {
                var port = portBase + portIndex;
                
                _logger.LogInformation("Registrando dispositivo: {DeviceName} (ID: {DeviceId}, Driver: {Driver}) na porta {Port}", 
                    device.Name, device.ID, device.Driver, port);

                _scannerRegistry.RegisterScanner(new ScannerInfo
                {
                    Id = device.ID,
                    Name = device.Name,
                    Device = device,
                    Port = port,
                    RegisteredAt = DateTime.UtcNow
                });

                // Registrar no servidor ESCL
                // O dispositivo já foi descoberto com as opções TWAIN corretas
                scanServer.RegisterDevice(device, displayName: device.Name, port: port);
                portIndex++;
            }

            // Definir o primeiro dispositivo como atual
            var firstDevice = devices.First();
            _logger.LogInformation("Dispositivo inicial selecionado: {DeviceName}", firstDevice.Name);

            _scannerManager.SetScanServer(scanServer);
            _scannerManager.SetCurrentDevice(firstDevice);

            // Share the device(s) until the service is stopped
            _logger.LogInformation("Iniciando servidor ESCL...");
            await scanServer.Start();
            _logger.LogInformation("=== SERVIDOR ESCL INICIADO COM SUCESSO ===");
            _logger.LogInformation("Porta ESCL base: http://localhost:9880/eSCL/");
            _logger.LogInformation("API de scanners: http://localhost:5000/api/scanners");
            _logger.LogInformation("Scanner atual: {DeviceName}", firstDevice.Name);
            _logger.LogInformation("Total de scanners disponíveis: {Count}", _scannerRegistry.Count);
            _logger.LogInformation("Todos os scanners estão acessíveis via suas portas ESCL");
            _logger.LogInformation("Use POST /api/scanners/{{id}}/select para trocar o scanner padrão");
            _logger.LogInformation("Aguardando requisições... (Pressione Ctrl+C para parar)");
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
            
            _logger.LogInformation("Sinal de parada recebido. Parando servidor ESCL...");
            await scanServer.Stop();
            _logger.LogInformation("Servidor ESCL parado com sucesso");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker cancelado normalmente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro fatal ao executar o Worker: {Message}", ex.Message);
            throw;
        }
    }
}