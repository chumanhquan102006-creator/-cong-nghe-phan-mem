using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class OCRUploadViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Image file")]
    public IFormFile? ImageFile { get; set; }

    [Required]
    public string Language { get; set; } = "eng";

    public static Dictionary<string, string> LanguageOptions { get; } = new()
    {
        { "eng", "English" },
        { "vie", "Vietnamese" }
    };
}
