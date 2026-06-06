using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class TextScan
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string InputText { get; set; } = string.Empty;

    public int WordCount { get; set; }

    public double OverallSimilarityScore { get; set; }

    [Required]
    [StringLength(20)]
    public string RiskLevel { get; set; } = "Low";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<TextScanMatch> Matches { get; set; } = new();
}
