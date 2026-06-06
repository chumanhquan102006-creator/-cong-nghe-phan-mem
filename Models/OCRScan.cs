using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class OCRScan
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ExtractedText { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Language { get; set; } = "eng";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
