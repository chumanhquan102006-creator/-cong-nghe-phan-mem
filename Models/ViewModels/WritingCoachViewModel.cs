using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class WritingCoachViewModel
{
    [Required(ErrorMessage = "Validation_WritingCoachModeRequired")]
    [Display(Name = "Mode")]
    public string Mode { get; set; } = "Brainstorm";

    [StringLength(300, ErrorMessage = "Validation_TopicMaxLength")]
    [Display(Name = "Topic")]
    public string Topic { get; set; } = string.Empty;

    [Display(Name = "Essay type")]
    public string EssayType { get; set; } = "Essay";

    [Display(Name = "Thesis Statement / Main Claim")]
    public string ThesisStatement { get; set; } = string.Empty;

    [Display(Name = "Additional instructions")]
    public string UserInput { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public static Dictionary<string, string> ModeOptions { get; } = new()
    {
        { "Brainstorm", "Brainstorm Ideas" },
        { "Outline", "Generate Outline" },
        { "CounterArgument", "Generate Counter-arguments" },
        { "ThesisImprover", "Improve Thesis Statement" }
    };

    public static List<string> EssayTypeOptions { get; } = new()
    {
        "Essay",
        "Literature Review",
        "Research Proposal",
        "Thesis / Graduation Paper",
        "Reflection"
    };

    public static Dictionary<string, string> LanguageOptions { get; } = new()
    {
        { "en", "English" },
        { "vi", "Vietnamese" }
    };
}
