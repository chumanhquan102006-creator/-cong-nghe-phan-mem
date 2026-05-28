using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class DocumentChatMessage
{
    public int Id { get; set; }

    public int DocumentId { get; set; }
    public Document? Document { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    public string? SourceSnippet { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
