using Tesseract;

namespace AcademicAIAssistant.Services;

public class OCRService
{
    private readonly IWebHostEnvironment _environment;

    public OCRService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string ExtractTextFromImage(string imagePath, string language)
    {
        if (!File.Exists(imagePath))
        {
            throw new InvalidOperationException("Image file was not found on the server.");
        }

        string safeLanguage = language is "eng" or "vie" ? language : "eng";
        string tessDataPath = Path.Combine(_environment.ContentRootPath, "tessdata");
        string languageDataFile = Path.Combine(tessDataPath, $"{safeLanguage}.traineddata");

        if (!File.Exists(languageDataFile))
        {
            throw new InvalidOperationException("OCR language data not found. Please add traineddata files to the tessdata folder.");
        }

        try
        {
            using var engine = new TesseractEngine(tessDataPath, safeLanguage, EngineMode.Default);
            using var image = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(image);

            return NormalizeText(page.GetText());
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("OCR engine failed to read this image. Please try a clearer image.", ex);
        }
    }

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line)));
    }
}
