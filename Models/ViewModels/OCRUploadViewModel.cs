using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class OCRUploadViewModel
{
    [Required(ErrorMessage = "Validation_TitleRequired")]
    [StringLength(200, ErrorMessage = "Validation_TitleMaxLength200")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_ImageFileRequired")]
    [Display(Name = "Image file")]
    public IFormFile? ImageFile { get; set; }

    [Required(ErrorMessage = "Validation_OcrLanguageRequired")]
    [Display(Name = "Language")]
    public string Language { get; set; } = "eng";

    public static Dictionary<string, string> LanguageOptions { get; } = new()
    {
        { "eng", "English" },
        { "vie", "Vietnamese" }
    };
}
