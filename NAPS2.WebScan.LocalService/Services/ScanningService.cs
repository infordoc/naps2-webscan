using System.Diagnostics;
using NAPS2.Images;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Serialization;
using NAPS2.WebScan.LocalService.Models;

namespace NAPS2.WebScan.LocalService.Services;

public class ScanningService
{
    private readonly ILogger<ScanningService> _logger;
    private readonly ScannerService _scannerService;

    public ScanningService(ILogger<ScanningService> logger, ScannerService scannerService)
    {
        _logger = logger;
        _scannerService = scannerService;
    }

    public async Task<ScanResponse> ScanAsync(ScanRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var device = _scannerService.GetScanDevice(request.ScannerId);
            if (device == null)
            {
                return new ScanResponse
                {
                    Success = false,
                    Error = request.ScannerId == null 
                        ? "No default scanner set" 
                        : $"Scanner not found: {request.ScannerId}"
                };
            }

            _logger.LogInformation("Starting scan with device: {DeviceName}", device.Name);

            var scanOptions = CreateScanOptions(request, device);
            var controller = new ScanController(_scannerService.ScanningContext);
            
            var images = new List<ProcessedImage>();
            
            if (request.MultiPage && request.ScanSource.Equals("Feeder", StringComparison.OrdinalIgnoreCase))
            {
                await foreach (var image in controller.Scan(scanOptions))
                {
                    images.Add(image);
                    if (images.Count >= request.MaxPages)
                    {
                        break;
                    }
                }
            }
            else
            {
                await foreach (var image in controller.Scan(scanOptions))
                {
                    images.Add(image);
                    break; // Single page only
                }
            }

            if (images.Count == 0)
            {
                return new ScanResponse
                {
                    Success = false,
                    Error = "No images were scanned"
                };
            }

            _logger.LogInformation("Scanned {Count} page(s)", images.Count);

            // Apply post-processing
            if (request.AutoDeskew)
            {
                images = await ApplyPostProcessing(images, request);
            }

            // Generate output file
            var (fileData, mimeType, extension) = await GenerateOutputFile(images, request);
            
            stopwatch.Stop();
            
            return new ScanResponse
            {
                Success = true,
                FileData = Convert.ToBase64String(fileData),
                FileName = $"{request.FileName}{extension}",
                FileSize = fileData.Length,
                MimeType = mimeType,
                PageCount = images.Count,
                ScanTime = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scan");
            return new ScanResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private ScanOptions CreateScanOptions(ScanRequest request, ScanDevice device)
    {
        var options = new ScanOptions
        {
            Device = device,
            Dpi = request.Dpi,
            PaperSource = request.ScanSource.Equals("Feeder", StringComparison.OrdinalIgnoreCase)
                ? PaperSource.Feeder
                : PaperSource.Flatbed
        };

        // Set bit depth based on color mode
        options.BitDepth = request.ColorMode switch
        {
            "BlackAndWhite" => BitDepth.BlackAndWhite,
            "Grayscale" => BitDepth.Grayscale,
            _ => BitDepth.Color
        };

        // Set page size
        if (!request.PageSize.Equals("Custom", StringComparison.OrdinalIgnoreCase))
        {
            options.PageSize = request.PageSize switch
            {
                "Letter" => PageSize.Letter,
                "Legal" => PageSize.Legal,
                _ => PageSize.A4
            };
        }

        return options;
    }

    private async Task<List<ProcessedImage>> ApplyPostProcessing(
        List<ProcessedImage> images, 
        ScanRequest request)
    {
        var processedImages = new List<ProcessedImage>();

        foreach (var image in images)
        {
            var processedImage = image;

            if (request.AutoDeskew)
            {
                _logger.LogDebug("Auto-deskew requested but not yet implemented");
                // Deskew functionality would be implemented here
                // using NAPS2.Images.Deskewer if available
            }

            if (request.BlankPageDetection)
            {
                _logger.LogDebug("Blank page detection requested but not yet implemented");
                // Blank page detection would be implemented here
            }

            processedImages.Add(processedImage);
        }

        return processedImages;
    }

    private async Task<(byte[] fileData, string mimeType, string extension)> GenerateOutputFile(
        List<ProcessedImage> images, 
        ScanRequest request)
    {
        var format = request.Format.ToUpperInvariant();
        
        switch (format)
        {
            case "PDF":
                return await GeneratePdf(images);
            
            case "JPEG":
            case "JPG":
                return await GenerateImageFile(images[0], ImageFileFormat.Jpeg, request.JpegQuality);
            
            case "PNG":
                return await GenerateImageFile(images[0], ImageFileFormat.Png, 100);
            
            case "TIFF":
                return await GenerateImageFile(images[0], ImageFileFormat.Tiff, 100);
            
            default:
                throw new ArgumentException($"Unsupported format: {request.Format}");
        }
    }

    private async Task<(byte[] fileData, string mimeType, string extension)> GeneratePdf(
        List<ProcessedImage> images)
    {
        using var memoryStream = new MemoryStream();
        var pdfExporter = new PdfExporter(_scannerService.ScanningContext);
        
        await pdfExporter.Export(memoryStream, images);
        
        return (memoryStream.ToArray(), "application/pdf", ".pdf");
    }

    private async Task<(byte[] fileData, string mimeType, string extension)> GenerateImageFile(
        ProcessedImage image, 
        ImageFileFormat format,
        int quality)
    {
        using var memoryStream = new MemoryStream();
        
        image.Save(memoryStream, format, new ImageSaveOptions
        {
            Quality = quality
        });
        
        var (mimeType, extension) = format switch
        {
            ImageFileFormat.Jpeg => ("image/jpeg", ".jpg"),
            ImageFileFormat.Png => ("image/png", ".png"),
            ImageFileFormat.Tiff => ("image/tiff", ".tiff"),
            _ => ("application/octet-stream", ".bin")
        };
        
        return (memoryStream.ToArray(), mimeType, extension);
    }
}
