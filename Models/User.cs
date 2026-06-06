using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    // Placeholder for the future login/register feature.
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Student";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Document> Documents { get; set; } = new();
    public List<Essay> Essays { get; set; } = new();
    public List<DocumentChatMessage> DocumentChatMessages { get; set; } = new();
    public List<OCRScan> OCRScans { get; set; } = new();
    public List<WritingCoachSession> WritingCoachSessions { get; set; } = new();
    public List<TextScan> TextScans { get; set; } = new();
    public List<ReferenceItem> ReferenceItems { get; set; } = new();
    public UserAISetting? AISetting { get; set; }
}
