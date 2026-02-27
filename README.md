# NAPS2.WebScan

NAPS2.WebScan is a Windows service that provides a RESTful API for scanning documents using TWAIN-compatible scanners. Built with [NAPS2.Sdk](https://www.naps2.com/sdk), it enables web applications to interact with local scanners.

## Features

- ✅ Complete REST API for scanner control and document scanning
- ✅ TWAIN scanner support via NAPS2.SDK
- ✅ Windows Service installation support
- ✅ Flexible scanning parameters (DPI, color mode, paper size, etc.)
- ✅ Multiple output formats (PDF, JPEG, PNG, TIFF)
- ✅ Multi-page scanning support (ADF/Feeder)
- ✅ CORS enabled for web application integration
- ✅ Comprehensive logging with Serilog

## Installation

### Prerequisites

- Windows 10/11 or Windows Server
- .NET 8.0 Runtime
- TWAIN-compatible scanner with drivers installed

### Install as Windows Service

1. Download or build the latest release
2. Extract files to a directory (e.g., `C:\NAPS2.WebScan`)
3. Run `install.bat` as Administrator
4. Select option `1` to install the service
5. The service will start automatically

### Manual Console Mode (for testing)

Run the executable directly:
```cmd
NAPS2.WebScan.LocalService.exe
```

Or use the install.bat menu:
```cmd
install.bat
```
Select option `5` to run in console mode.

## Configuration

Edit `appsettings.json` to customize the service:

```json
{
  "Server": {
    "Port": 8080,
    "Hostname": "localhost"
  },
  "Cors": {
    "Enabled": true,
    "AllowedOrigins": ["*"]
  },
  "Scanning": {
    "DefaultDpi": 300,
    "DefaultColorMode": "Color",
    "DefaultFormat": "PDF",
    "TempDirectory": "./temp",
    "MaxFileSize": 52428800
  }
}
```

**⚠️ Security Warning**: By default, CORS is configured to allow all origins (`"*"`). This is convenient for development but poses a security risk in production. Unauthorized web applications could scan documents without user consent. For production deployments, restrict `AllowedOrigins` to specific trusted domains:

```json
{
  "Cors": {
    "Enabled": true,
    "AllowedOrigins": ["https://your-trusted-domain.com", "https://intranet.company.com"]
  }
}
```

## API Documentation

Base URL: `http://localhost:8080`

### 1. Health Check

Check if the service is running:

```
GET /api/health
```

Response:
```json
{
  "Status": "Healthy",
  "Service": "NAPS2 WebScan Server",
  "Version": "1.0.0",
  "Timestamp": "2024-01-01T00:00:00.000Z"
}
```

### 2. List Scanners

Get list of available TWAIN scanners:

```
GET /api/scanners
```

Response:
```json
{
  "scanners": [
    {
      "id": "Twain_Scanner-ID",
      "name": "HP LaserJet Scanner",
      "driver": "Twain",
      "isDefault": true,
      "capabilities": {
        "supportedResolutions": [100, 150, 200, 300, 600, 1200],
        "supportedColorModes": ["Color", "Grayscale", "BlackAndWhite"],
        "supportedPageSizes": ["A4", "Letter", "Legal"],
        "hasADF": true,
        "hasFlatbed": true
      }
    }
  ]
}
```

### 3. Get Default Scanner

Get the currently configured default scanner:

```
GET /api/scanners/default
```

Response:
```json
{
  "success": true,
  "data": {
    "id": "Twain_Scanner-ID",
    "name": "HP LaserJet Scanner",
    "driver": "Twain",
    "isDefault": true
  }
}
```

### 4. Set Default Scanner

Set the default scanner for operations:

```
POST /api/scanners/default
Content-Type: application/json

{
  "scannerId": "Twain_Scanner-ID"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "scannerId": "Twain_Scanner-ID"
  }
}
```

### 5. Scan Document

Scan a document with customizable parameters:

```
POST /api/scan
Content-Type: application/json

{
  "scannerId": "Twain_Scanner-ID",
  "dpi": 300,
  "colorMode": "Color",
  "pageSize": "A4",
  "scanSource": "Flatbed",
  "brightness": 0,
  "contrast": 0,
  "format": "PDF",
  "multiPage": false,
  "maxPages": 1,
  "fileName": "scan",
  "jpegQuality": 90
}
```

#### Parameters

All parameters are optional with sensible defaults:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| scannerId | string | (default) | Scanner ID from /api/scanners |
| dpi | int | 300 | Resolution: 100, 150, 200, 300, 600, 1200 |
| colorMode | string | "Color" | "Color", "Grayscale", "BlackAndWhite" |
| pageSize | string | "A4" | "A4", "Letter", "Legal", "Custom" |
| scanSource | string | "Flatbed" | "Flatbed", "Feeder" |
| brightness | int | 0 | -100 to 100 (reserved for future use) |
| contrast | int | 0 | -100 to 100 (reserved for future use) |
| format | string | "PDF" | "PDF", "JPEG", "PNG", "TIFF" |
| multiPage | bool | false | Scan multiple pages (use with Feeder) |
| maxPages | int | 1 | Maximum pages to scan |
| fileName | string | "scan" | Base filename for output |
| jpegQuality | int | 90 | JPEG quality (1-100) |

**Note**: Scanner capabilities shown in `/api/scanners` are currently indicative. Actual supported values may vary by device.

Response:
```json
{
  "success": true,
  "fileData": "base64-encoded-file-content",
  "fileName": "scan.pdf",
  "fileSize": 123456,
  "mimeType": "application/pdf",
  "pageCount": 1,
  "scanTime": 2500
}
```

### 6. Get Configuration

Get current service configuration:

```
GET /api/config
```

Response:
```json
{
  "server": {
    "port": 8080,
    "hostname": "localhost"
  },
  "cors": {
    "enabled": true,
    "allowedOrigins": ["*"]
  },
  "scanning": {
    "defaultDpi": 300,
    "defaultColorMode": "Color",
    "defaultFormat": "PDF",
    "tempDirectory": "./temp",
    "maxFileSize": 52428800
  }
}
```

## Usage Examples

### JavaScript/TypeScript

```javascript
// List scanners
const scanners = await fetch('http://localhost:8080/api/scanners')
  .then(res => res.json());

// Set default scanner
await fetch('http://localhost:8080/api/scanners/default', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ scannerId: 'Twain_Scanner-ID' })
});

// Scan a document
const scanResult = await fetch('http://localhost:8080/api/scan', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    dpi: 300,
    colorMode: 'Color',
    format: 'PDF',
    fileName: 'my-scan'
  })
}).then(res => res.json());

// Download the scanned file
const blob = base64ToBlob(scanResult.fileData, scanResult.mimeType);
const url = URL.createObjectURL(blob);
const a = document.createElement('a');
a.href = url;
a.download = scanResult.fileName;
a.click();
```

### Python

```python
import requests
import base64

# List scanners
response = requests.get('http://localhost:8080/api/scanners')
scanners = response.json()['scanners']

# Scan a document
scan_request = {
    'dpi': 300,
    'colorMode': 'Color',
    'format': 'PDF',
    'fileName': 'document'
}
response = requests.post('http://localhost:8080/api/scan', json=scan_request)
result = response.json()

# Save the scanned file
if result['success']:
    file_data = base64.b64decode(result['fileData'])
    with open(result['fileName'], 'wb') as f:
        f.write(file_data)
```

### C#

```csharp
using System.Net.Http.Json;
using System.Text.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };

// List scanners
var scanners = await client.GetFromJsonAsync<ScannersResponse>("/api/scanners");

// Scan a document
var scanRequest = new ScanRequest
{
    Dpi = 300,
    ColorMode = "Color",
    Format = "PDF",
    FileName = "document"
};
var response = await client.PostAsJsonAsync("/api/scan", scanRequest);
var result = await response.Content.ReadFromJsonAsync<ScanResponse>();

// Save the file
if (result.Success)
{
    var fileData = Convert.FromBase64String(result.FileData);
    await File.WriteAllBytesAsync(result.FileName, fileData);
}
```

## Service Management

### Using install.bat

The `install.bat` script provides an interactive menu for managing the service:

1. **Install as service** - Registers the Windows service
2. **Uninstall service** - Removes the Windows service
3. **Start service** - Starts the service
4. **Stop service** - Stops the service
5. **Run in console mode** - Run directly for testing

### Using Windows Services Manager

1. Open `services.msc`
2. Find "NAPS2 WebScan Server"
3. Right-click to Start/Stop/Restart

### Using Command Line

```cmd
# Start service
sc start NAPS2WebScanServer

# Stop service
sc stop NAPS2WebScanServer

# Check service status
sc query NAPS2WebScanServer
```

## Troubleshooting

### Scanner Not Found

- Ensure TWAIN drivers are properly installed
- Verify scanner is connected and powered on
- Check Device Manager for scanner status
- Restart the service after connecting a new scanner

### Service Won't Start

- Check logs in the `logs/` directory
- Verify port 8080 is not in use by another application
- Run in console mode to see detailed error messages
- Ensure .NET 8.0 Runtime is installed

### Permission Errors

- Run install.bat as Administrator
- Ensure the user account running the service has scanner access
- Check Windows Event Viewer for detailed error messages

### CORS Issues

- Ensure CORS is enabled in appsettings.json
- Verify AllowedOrigins includes your domain or "*"
- Check browser console for CORS-related errors

## Security Considerations

### CORS Configuration

The default CORS configuration allows **all origins** (`"*"`), which is convenient for development but poses security risks in production:

- Any website could potentially trigger scans through your service
- Unauthorized applications could access your scanner
- Documents could be scanned without explicit user consent

**Recommended Production Configuration:**

```json
{
  "Cors": {
    "Enabled": true,
    "AllowedOrigins": [
      "https://your-intranet.company.com",
      "https://trusted-app.company.com"
    ]
  }
}
```

### Network Exposure

- By default, the service binds to `localhost` only
- This prevents external network access
- Only applications running on the same machine can access the service
- This is the recommended configuration for most use cases

### Scanner Access Control

- The service inherits scanner permissions from the Windows user account running it
- Ensure only authorized users can run the service
- Consider using a dedicated service account with appropriate permissions

## Build from Source

### Prerequisites

- .NET 8.0 SDK
- Windows 10/11 or Windows Server

### Build Steps

```cmd
# Clone the repository
git clone https://github.com/infordoc/naps2-webscan
cd naps2-webscan

# Build the project
dotnet build NAPS2.WebScan.LocalService/NAPS2.WebScan.LocalService.csproj

# Publish for release
dotnet publish NAPS2.WebScan.LocalService/NAPS2.WebScan.LocalService.csproj -c Release
```

## Architecture

The service is organized into the following components:

- **Controllers**: Handle HTTP requests and responses
  - `ScannersController.cs` - Scanner discovery and configuration
  - `ScanController.cs` - Document scanning operations
  
- **Services**: Business logic
  - `ScannerService.cs` - Scanner management and TWAIN device listing
  - `ScanningService.cs` - Scanning operations and file generation
  
- **Models**: Data transfer objects
  - `ScannerInfo.cs` - Scanner information and capabilities
  - `ScanRequest.cs` - Scan operation parameters
  - `ScanResponse.cs` - Scan operation results

## Technology Stack

- [.NET 8.0](https://dotnet.microsoft.com/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [NAPS2.SDK](https://www.naps2.com/sdk) - Scanner interaction
- [Serilog](https://serilog.net/) - Logging

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions:
- Create an issue on [GitHub](https://github.com/infordoc/naps2-webscan/issues)
- Check NAPS2 documentation at [naps2.com](https://www.naps2.com/)

## Acknowledgments

- Built on [NAPS2.Sdk](https://www.naps2.com/sdk) by [cyanfish](https://github.com/cyanfish)
- Inspired by the [Zebra Print Server](https://github.com/infordoc/Zebra-Print-Server) project
