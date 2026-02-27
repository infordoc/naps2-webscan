namespace NAPS2.WebScan.LocalService.Models;

public class ScanResponse
{
    public bool Success { get; set; }
    public string? FileData { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public int PageCount { get; set; }
    public long ScanTime { get; set; }
    public string? Error { get; set; }
}
