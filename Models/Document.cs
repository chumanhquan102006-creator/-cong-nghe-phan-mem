using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class Document
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

    public long FileSize { get; set; }

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public string? ExtractedText { get; set; }

    public DateTime? ExtractedAt { get; set; }

    public string? Summary { get; set; }

    public DateTime? SummaryGeneratedAt { get; set; }

    public int WordCount { get; set; }

    public int PageCount { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.Now;

    public List<SimilarityMatch> SimilarityMatches { get; set; } = new();

    public List<DocumentChatMessage> ChatMessages { get; set; } = new();
}
