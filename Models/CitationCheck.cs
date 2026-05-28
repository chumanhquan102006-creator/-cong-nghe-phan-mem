using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class CitationCheck
{
    public int Id { get; set; }

    public int EssayId { get; set; }
    public Essay? Essay { get; set; }

    public int TotalInTextCitations { get; set; }

    public int TotalReferences { get; set; }

    public string MissingReferences { get; set; } = string.Empty;

    public string UnusedReferences { get; set; } = string.Empty;

    public string FormatIssues { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string OverallStatus { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; } = DateTime.Now;
}
