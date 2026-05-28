namespace AcademicAIAssistant.Models.ViewModels;

public class EssayDetailsViewModel
{
    public Essay Essay { get; set; } = new();

    public FeedbackReport? LatestFeedbackReport { get; set; }
}
