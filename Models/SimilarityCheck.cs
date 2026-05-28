using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class SimilarityCheck
{
    public int Id { get; set; }

    public int EssayId { get; set; }
    public Essay? Essay { get; set; }

    public double OverallSimilarityScore { get; set; }

    public int TotalDocumentsCompared { get; set; }

    public int TotalMatches { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Low";

    public DateTime CheckedAt { get; set; } = DateTime.Now;

    public List<SimilarityMatch> Matches { get; set; } = new();
}
