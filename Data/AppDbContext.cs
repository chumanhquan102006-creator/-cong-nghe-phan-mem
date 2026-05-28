using AcademicAIAssistant.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicAIAssistant.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Essay> Essays { get; set; }
    public DbSet<FeedbackReport> FeedbackReports { get; set; }
    public DbSet<CitationCheck> CitationChecks { get; set; }
    public DbSet<SimilarityCheck> SimilarityChecks { get; set; }
    public DbSet<SimilarityMatch> SimilarityMatches { get; set; }
    public DbSet<GraphNode> GraphNodes { get; set; }
    public DbSet<GraphEdge> GraphEdges { get; set; }
    public DbSet<DocumentChatMessage> DocumentChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<Essay>()
            .HasMany(essay => essay.FeedbackReports)
            .WithOne(report => report.Essay)
            .HasForeignKey(report => report.EssayId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Essay>()
            .HasMany(essay => essay.CitationChecks)
            .WithOne(check => check.Essay)
            .HasForeignKey(check => check.EssayId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Essay>()
            .HasMany(essay => essay.SimilarityChecks)
            .WithOne(check => check.Essay)
            .HasForeignKey(check => check.EssayId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SimilarityCheck>()
            .HasMany(check => check.Matches)
            .WithOne(match => match.SimilarityCheck)
            .HasForeignKey(match => match.SimilarityCheckId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Document>()
            .HasMany(document => document.SimilarityMatches)
            .WithOne(match => match.Document)
            .HasForeignKey(match => match.DocumentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<GraphEdge>()
            .HasOne(edge => edge.SourceNode)
            .WithMany(node => node.OutgoingEdges)
            .HasForeignKey(edge => edge.SourceNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GraphEdge>()
            .HasOne(edge => edge.TargetNode)
            .WithMany(node => node.IncomingEdges)
            .HasForeignKey(edge => edge.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasMany(document => document.ChatMessages)
            .WithOne(message => message.Document)
            .HasForeignKey(message => message.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(user => user.DocumentChatMessages)
            .WithOne(message => message.User)
            .HasForeignKey(message => message.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
