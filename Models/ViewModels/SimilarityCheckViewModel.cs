namespace AcademicAIAssistant.Models.ViewModels;

public class SimilarityCheckViewModel
{
    public Essay Essay { get; set; } = new();

    public int ExtractedDocumentCount { get; set; }

    public string? ErrorMessage { get; set; }
}
