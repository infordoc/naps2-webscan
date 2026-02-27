namespace NAPS2.WebScan.LocalService.Models;

public class ServerConfig
{
    public int Port { get; set; } = 8080;
    public string Hostname { get; set; } = "localhost";
}

public class CorsConfig
{
    public bool Enabled { get; set; } = true;
    public List<string> AllowedOrigins { get; set; } = new() { "*" };
}

public class ScanningConfig
{
    public int DefaultDpi { get; set; } = 300;
    public string DefaultColorMode { get; set; } = "Color";
    public string DefaultFormat { get; set; } = "PDF";
    public string TempDirectory { get; set; } = "./temp";
    public long MaxFileSize { get; set; } = 52428800;
}

public class AppConfig
{
    public ServerConfig Server { get; set; } = new();
    public CorsConfig Cors { get; set; } = new();
    public ScanningConfig Scanning { get; set; } = new();
}
