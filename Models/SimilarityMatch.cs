using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class SimilarityMatch
{
    public int Id { get; set; }

    public int SimilarityCheckId { get; set; }
    public SimilarityCheck? SimilarityCheck { get; set; }

    public int DocumentId { get; set; }
    public Document? Document { get; set; }

    [Required]
    public string EssaySegment { get; set; } = string.Empty;

    [Required]
    public string MatchedText { get; set; } = string.Empty;

    public double SimilarityScore { get; set; }

    public int? PageNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
