using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class EssayAnalyzeViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Essay type")]
    public string EssayType { get; set; } = "Essay";

    [Required]
    [MinLength(100, ErrorMessage = "Content should be at least 100 characters.")]
    public string Content { get; set; } = string.Empty;

    public static List<string> EssayTypeOptions { get; } = new()
    {
        "Essay",
        "Literature Review",
        "Thesis Proposal",
        "Research Report"
    };
}
