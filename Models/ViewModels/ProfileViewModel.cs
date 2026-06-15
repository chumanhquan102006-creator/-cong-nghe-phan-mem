namespace AcademicAIAssistant.Models.ViewModels;

public class ProfileViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int TotalDocuments { get; set; }

    public int TotalEssays { get; set; }

    public int TotalOcrScans { get; set; }

    public int TotalTextScans { get; set; }

    public int TotalWritingCoachSessions { get; set; }

    public int TotalReferences { get; set; }
}
