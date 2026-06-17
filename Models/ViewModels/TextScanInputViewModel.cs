using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class TextScanInputViewModel
{
    [Required(ErrorMessage = "Validation_TitleRequired")]
    [StringLength(200, ErrorMessage = "Validation_TitleMaxLength200")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_TextScanInputRequired")]
    [Display(Name = "Text to scan")]
    public string InputText { get; set; } = string.Empty;
}
