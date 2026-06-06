namespace AcademicAIAssistant.Services;

public class FileValidationService : IFileValidationService
{
    private const long MaxPdfSizeBytes = 10 * 1024 * 1024;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png"
    };

    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png"
    };

    public FileValidationResult ValidatePdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return FileValidationResult.Failure("Upload_SelectPdf");
        }

        if (file.Length > MaxPdfSizeBytes)
        {
            return FileValidationResult.Failure("Upload_PdfTooLarge");
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            return FileValidationResult.Failure("Upload_OnlyPdfAllowed");
        }

        bool contentTypeLooksPdf = string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(file.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);

        if (!contentTypeLooksPdf || !HasPdfSignature(file))
        {
            return FileValidationResult.Failure("Upload_InvalidPdfFile");
        }

        return FileValidationResult.Success(".pdf");
    }

    public FileValidationResult ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return FileValidationResult.Failure("Upload_SelectImage");
        }

        if (file.Length > MaxImageSizeBytes)
        {
            return FileValidationResult.Failure("Upload_ImageTooLarge");
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
        {
            return FileValidationResult.Failure("Upload_OnlyImagesAllowed");
        }

        if (!AllowedImageContentTypes.Contains(file.ContentType))
        {
            return FileValidationResult.Failure("Upload_InvalidImageFile");
        }

        if (!ImageSignatureMatchesExtension(file, extension))
        {
            return FileValidationResult.Failure("Upload_InvalidImageFile");
        }

        return FileValidationResult.Success(extension == ".jpeg" ? ".jpg" : extension);
    }

    private static bool HasPdfSignature(IFormFile file)
    {
        byte[] buffer = ReadHeader(file, 4);
        return buffer.Length >= 4
            && buffer[0] == 0x25
            && buffer[1] == 0x50
            && buffer[2] == 0x44
            && buffer[3] == 0x46;
    }

    private static bool ImageSignatureMatchesExtension(IFormFile file, string extension)
    {
        byte[] buffer = ReadHeader(file, 8);

        bool isPng = buffer.Length >= 4
            && buffer[0] == 0x89
            && buffer[1] == 0x50
            && buffer[2] == 0x4E
            && buffer[3] == 0x47;

        bool isJpeg = buffer.Length >= 3
            && buffer[0] == 0xFF
            && buffer[1] == 0xD8
            && buffer[2] == 0xFF;

        return extension switch
        {
            ".png" => isPng,
            ".jpg" or ".jpeg" => isJpeg,
            _ => false
        };
    }

    private static byte[] ReadHeader(IFormFile file, int byteCount)
    {
        byte[] buffer = new byte[byteCount];
        using Stream stream = file.OpenReadStream();
        int bytesRead = stream.Read(buffer, 0, byteCount);

        if (bytesRead == byteCount)
        {
            return buffer;
        }

        return buffer.Take(bytesRead).ToArray();
    }
}
