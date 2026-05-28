namespace AcademicAIAssistant.Services;

public class PdfTextExtractionResult
{
    public string Text { get; set; } = string.Empty;

    public int PageCount { get; set; }

    public int WordCount { get; set; }
}
