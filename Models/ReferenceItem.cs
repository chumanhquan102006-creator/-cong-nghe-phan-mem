using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class ReferenceItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(50)]
    public string SourceType { get; set; } = "Book";

    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(300)]
    public string Author { get; set; } = string.Empty;

    [StringLength(20)]
    public string Year { get; set; } = string.Empty;

    [StringLength(300)]
    public string JournalOrPublisher { get; set; } = string.Empty;

    [StringLength(300)]
    public string WebsiteName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [StringLength(200)]
    public string Doi { get; set; } = string.Empty;

    [StringLength(50)]
    public string Volume { get; set; } = string.Empty;

    [StringLength(50)]
    public string Issue { get; set; } = string.Empty;

    [StringLength(50)]
    public string Pages { get; set; } = string.Empty;

    public DateTime? AccessDate { get; set; }

    public string ApaInTextCitation { get; set; } = string.Empty;

    public string ApaReference { get; set; } = string.Empty;

    public string MlaInTextCitation { get; set; } = string.Empty;

    public string MlaReference { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}
