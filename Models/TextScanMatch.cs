using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class TextScanMatch
{
    public int Id { get; set; }

    public int TextScanId { get; set; }
    public TextScan? TextScan { get; set; }

    [Required]
    [StringLength(50)]
    public string SourceType { get; set; } = string.Empty;

    public int SourceId { get; set; }

    [Required]
    [StringLength(300)]
    public string SourceTitle { get; set; } = string.Empty;

    public string InputSegment { get; set; } = string.Empty;

    public string MatchedSegment { get; set; } = string.Empty;

    public double SimilarityScore { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
