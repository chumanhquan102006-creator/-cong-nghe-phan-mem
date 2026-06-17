using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class ReferenceItemFormViewModel
{
    [Required(ErrorMessage = "Validation_ReferenceSourceTypeRequired")]
    [Display(Name = "Source Type")]
    public string SourceType { get; set; } = "Book";

    [Required(ErrorMessage = "Validation_ReferenceTitleRequired")]
    [StringLength(300, ErrorMessage = "Validation_ReferenceTitleMaxLength")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Validation_ReferenceAuthorMaxLength")]
    [Display(Name = "Author")]
    public string Author { get; set; } = string.Empty;

    [RegularExpression(@"^\d{4}$|^$", ErrorMessage = "Validation_ReferenceYearInvalid")]
    [Display(Name = "Year")]
    public string Year { get; set; } = string.Empty;

    [Display(Name = "Journal / Publisher")]
    public string JournalOrPublisher { get; set; } = string.Empty;

    [Display(Name = "Website Name")]
    public string WebsiteName { get; set; } = string.Empty;

    [Url(ErrorMessage = "Validation_ReferenceUrlInvalid")]
    [Display(Name = "Url")]
    public string Url { get; set; } = string.Empty;

    public string Doi { get; set; } = string.Empty;

    public string Volume { get; set; } = string.Empty;

    public string Issue { get; set; } = string.Empty;

    public string Pages { get; set; } = string.Empty;

    [Display(Name = "Access Date")]
    public DateTime? AccessDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public static List<string> SourceTypeOptions { get; } = new()
    {
        "Book",
        "JournalArticle",
        "Website",
        "ConferencePaper",
        "Report",
        "Other"
    };
}
