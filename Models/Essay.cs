using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class Essay
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string EssayType { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public int WordCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public List<FeedbackReport> FeedbackReports { get; set; } = new();

    public List<CitationCheck> CitationChecks { get; set; } = new();

    public List<SimilarityCheck> SimilarityChecks { get; set; } = new();
}
