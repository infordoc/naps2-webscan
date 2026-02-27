namespace NAPS2.WebScan.LocalService.Models;

public class ScanRequest
{
    public string? ScannerId { get; set; }
    public int Dpi { get; set; } = 300;
    public string ColorMode { get; set; } = "Color";
    public string PageSize { get; set; } = "A4";
    public string ScanSource { get; set; } = "Flatbed";
    public int Brightness { get; set; } = 0;
    public int Contrast { get; set; } = 0;
    public string Format { get; set; } = "PDF";
    public bool MultiPage { get; set; } = false;
    public int MaxPages { get; set; } = 1;
    public string FileName { get; set; } = "scan";
    public int JpegQuality { get; set; } = 90;
}
