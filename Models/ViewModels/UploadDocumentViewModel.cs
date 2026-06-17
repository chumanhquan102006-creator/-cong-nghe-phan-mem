using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class UploadDocumentViewModel
{
    [Required(ErrorMessage = "Validation_TitleRequired")]
    [StringLength(200, ErrorMessage = "Validation_TitleMaxLength200")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_PdfFileRequired")]
    [Display(Name = "PDF file")]
    public IFormFile? File { get; set; }
}
