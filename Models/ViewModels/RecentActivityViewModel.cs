namespace AcademicAIAssistant.Models.ViewModels;

public class RecentActivityViewModel
{
    public string ActivityType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string LinkUrl { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi bi-clock-history";
    public string BadgeClass { get; set; } = "text-bg-secondary";
}
