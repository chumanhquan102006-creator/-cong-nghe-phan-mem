using System.Text.RegularExpressions;

namespace AcademicAIAssistant.Models.ViewModels;

public class DocumentWorkspaceViewModel
{
    public const string OverviewTab = "overview";
    public const string SummaryTab = "summary";
    public const string ChatTab = "chat";

    public Document Document { get; set; } = new();

    public List<DocumentChatMessage> Messages { get; set; } = new();

    public string ActiveTab { get; set; } = OverviewTab;

    public bool FileExists { get; set; }

    public bool HasExtractedText => !string.IsNullOrWhiteSpace(Document.ExtractedText);

    public bool HasSummary => !string.IsNullOrWhiteSpace(Document.Summary);

    public bool IsOverviewActive => ActiveTab == OverviewTab;

    public bool IsSummaryActive => ActiveTab == SummaryTab;

    public bool IsChatActive => ActiveTab == ChatTab;

    public string SummaryPreview => BuildSummaryPreview(Document.Summary);

    public static string NormalizeTab(string? tab)
    {
        return tab?.Trim().ToLowerInvariant() switch
        {
            SummaryTab => SummaryTab,
            ChatTab => ChatTab,
            _ => OverviewTab
        };
    }

    private static string BuildSummaryPreview(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return string.Empty;
        }

        string normalizedSummary = Regex.Replace(summary, @"\s+", " ").Trim();
        string[] sentences = Regex.Split(normalizedSummary, @"(?<=[.!?])\s+")
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence))
            .Take(3)
            .ToArray();

        string preview = sentences.Length == 0
            ? normalizedSummary
            : string.Join(" ", sentences);

        return preview.Length <= 520 ? preview : preview[..520] + "...";
    }
}
