using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class GraphNode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string NodeType { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string SourceType { get; set; } = string.Empty;

    public int? SourceId { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<GraphEdge> OutgoingEdges { get; set; } = new();

    public List<GraphEdge> IncomingEdges { get; set; } = new();
}
