using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class EssayAnalyzeViewModel
{
    [Required(ErrorMessage = "Validation_TitleRequired")]
    [StringLength(200, ErrorMessage = "Validation_TitleMaxLength200")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_EssayTypeRequired")]
    [Display(Name = "Essay type")]
    public string EssayType { get; set; } = "Essay";

    [Required(ErrorMessage = "Validation_EssayContentRequired")]
    [MinLength(100, ErrorMessage = "Validation_EssayContentMinLength")]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    public static List<string> EssayTypeOptions { get; } = new()
    {
        "Essay",
        "Literature Review",
        "Thesis Proposal",
        "Research Report"
    };
}
