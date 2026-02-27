namespace NAPS2.WebScan.LocalService.Models;

public class ScannerInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string Driver { get; set; } = "TWAIN";
    public bool IsDefault { get; set; }
    public ScannerCapabilities? Capabilities { get; set; }
}

public class ScannerCapabilities
{
    public List<int>? SupportedResolutions { get; set; }
    public List<string>? SupportedColorModes { get; set; }
    public List<string>? SupportedPageSizes { get; set; }
    public bool HasADF { get; set; }
    public bool HasFlatbed { get; set; }
}

public class ScannersResponse
{
    public List<ScannerInfo> Scanners { get; set; } = new();
}
