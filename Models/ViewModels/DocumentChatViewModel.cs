namespace AcademicAIAssistant.Models.ViewModels;

public class DocumentChatViewModel
{
    public Document Document { get; set; } = new();

    public List<DocumentChatMessage> Messages { get; set; } = new();

    public string? Question { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ReturnTo { get; set; }
}
