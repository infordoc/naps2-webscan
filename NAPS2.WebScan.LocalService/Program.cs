using NAPS2.WebScan.LocalService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure for Windows Service
if (args.Contains("--service"))
{
    builder.Host.UseWindowsService();
}

// Configure Kestrel to listen on the configured port
var serverPort = builder.Configuration.GetValue<int>("Server:Port", 8080);
builder.WebHost.UseUrls($"http://localhost:{serverPort}");

// Add services
builder.Services.AddControllers();
builder.Services.AddSingleton<ScannerService>();
builder.Services.AddSingleton<ScanningService>();

// Configure CORS
var corsEnabled = builder.Configuration.GetValue<bool>("Cors:Enabled", true);
if (corsEnabled)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

var app = builder.Build();

// Use CORS
if (corsEnabled)
{
    app.UseCors();
}

// Map controllers
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => Results.Text(@"
<!DOCTYPE html>
<html>
<head>
    <title>NAPS2 WebScan Server</title>
</head>
<body>
    <h1>NAPS2 WebScan Server</h1>
    <p>Service is running. API available at:</p>
    <ul>
        <li>GET /api/health - Check service status</li>
        <li>GET /api/scanners - List available scanners</li>
        <li>GET /api/scanners/default - Get default scanner</li>
        <li>POST /api/scanners/default - Set default scanner</li>
        <li>POST /api/scan - Scan a document</li>
    </ul>
</body>
</html>", "text/html"));

Log.Information($"NAPS2 WebScan Server starting on http://localhost:{serverPort}");

app.Run();
