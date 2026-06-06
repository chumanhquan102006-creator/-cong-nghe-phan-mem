using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class WritingCoachSession
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(50)]
    public string Mode { get; set; } = "Brainstorm";

    [StringLength(300)]
    public string Topic { get; set; } = string.Empty;

    [StringLength(100)]
    public string EssayType { get; set; } = "Essay";

    public string ThesisStatement { get; set; } = string.Empty;

    public string UserInput { get; set; } = string.Empty;

    public string AIResponse { get; set; } = string.Empty;

    [Required]
    [StringLength(5)]
    public string Language { get; set; } = "en";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
