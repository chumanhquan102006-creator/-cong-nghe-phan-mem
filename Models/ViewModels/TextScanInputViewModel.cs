using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class TextScanInputViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Text to scan")]
    public string InputText { get; set; } = string.Empty;
}
