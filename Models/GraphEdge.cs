using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models;

public class GraphEdge
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SourceNodeId { get; set; }
    public GraphNode? SourceNode { get; set; }

    public int TargetNodeId { get; set; }
    public GraphNode? TargetNode { get; set; }

    [Required]
    [StringLength(50)]
    public string RelationType { get; set; } = string.Empty;

    public double Weight { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
