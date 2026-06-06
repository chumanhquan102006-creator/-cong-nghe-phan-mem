using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class UserAISetting
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = "OpenAI";

    // Demo note: production systems should encrypt API keys before storing them.
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ModelName { get; set; } = "gpt-4o-mini";

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}
