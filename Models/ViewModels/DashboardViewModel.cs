namespace AcademicAIAssistant.Models.ViewModels;

public class DashboardViewModel
{
    public string FullName { get; set; } = string.Empty;

    public int TotalDocuments { get; set; }
    public int ExtractedDocuments { get; set; }
    public int SummarizedDocuments { get; set; }
    public int TotalEssays { get; set; }
    public int TotalFeedbackReports { get; set; }
    public int TotalCitationChecks { get; set; }
    public int TotalSimilarityChecks { get; set; }
    public int TotalGraphNodes { get; set; }
    public int TotalGraphEdges { get; set; }

    public List<Document> RecentDocuments { get; set; } = new();
    public List<Essay> RecentEssays { get; set; } = new();
    public List<CitationCheck> RecentCitationChecks { get; set; } = new();
    public List<SimilarityCheck> RecentSimilarityChecks { get; set; } = new();
    public List<DocumentChatMessage> RecentChatMessages { get; set; } = new();
    public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
}
