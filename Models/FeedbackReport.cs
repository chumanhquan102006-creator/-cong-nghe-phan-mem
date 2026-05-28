using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class FeedbackReport
{
    public int Id { get; set; }

    public int EssayId { get; set; }
    public Essay? Essay { get; set; }

    public int OverallScore { get; set; }

    [Required]
    public string GrammarFeedback { get; set; } = string.Empty;

    [Required]
    public string AcademicToneFeedback { get; set; } = string.Empty;

    [Required]
    public string ThesisFeedback { get; set; } = string.Empty;

    [Required]
    public string StructureFeedback { get; set; } = string.Empty;

    [Required]
    public string LogicFeedback { get; set; } = string.Empty;

    [Required]
    public string CitationFeedback { get; set; } = string.Empty;

    [Required]
    public string GeneralSuggestions { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
