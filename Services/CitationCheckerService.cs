using System.Text.RegularExpressions;
using AcademicAIAssistant.Models;

namespace AcademicAIAssistant.Services;

public class CitationCheckerService
{
    public CitationCheck CheckCitation(string essayContent)
    {
        string content = NormalizeText(essayContent);
        var issues = new List<string>();

        var referencesInfo = SplitReferencesSection(content);
        string bodyText = referencesInfo.BodyText;
        string referencesText = referencesInfo.ReferencesText;

        var inTextCitations = FindInTextCitations(bodyText).Distinct().ToList();
        var references = FindReferences(referencesText).ToList();

        if (!referencesInfo.HasReferencesSection)
        {
            issues.Add("No References section found.");
        }
        else if (string.IsNullOrWhiteSpace(referencesText))
        {
            issues.Add("References section is empty.");
        }

        issues.AddRange(FindFormatIssues(bodyText, referencesText));

        var missingReferences = inTextCitations
            .Where(citation => !references.Any(reference => MatchesCitation(reference, citation)))
            .Select(citation => $"{citation.Author}, {citation.Year}")
            .Distinct()
            .ToList();

        var unusedReferences = references
            .Where(reference => !inTextCitations.Any(citation => MatchesCitation(reference, citation)))
            .Select(reference => $"{reference.Author}, {reference.Year}")
            .Distinct()
            .ToList();

        string overallStatus = GetOverallStatus(
            referencesInfo.HasReferencesSection,
            referencesText,
            issues,
            missingReferences,
            unusedReferences);

        return new CitationCheck
        {
            TotalInTextCitations = inTextCitations.Count,
            TotalReferences = references.Count,
            MissingReferences = ToMultilineText(missingReferences),
            UnusedReferences = ToMultilineText(unusedReferences),
            FormatIssues = ToMultilineText(issues),
            OverallStatus = overallStatus,
            CheckedAt = DateTime.Now
        };
    }

    private static List<CitationItem> FindInTextCitations(string bodyText)
    {
        var citations = new List<CitationItem>();

        // Examples: (Smith, 2020), (Nguyen & Tran, 2021), (Lee et al., 2022)
        foreach (Match match in Regex.Matches(bodyText, @"\(([A-Z][A-Za-z]+)(?:\s*(?:&|and)\s*[A-Z][A-Za-z]+|\s+et al\.)?,\s*(\d{4})\)"))
        {
            citations.Add(new CitationItem(match.Groups[1].Value, match.Groups[2].Value));
        }

        // Examples: Smith (2020), Nguyen and Tran (2021)
        foreach (Match match in Regex.Matches(bodyText, @"\b([A-Z][A-Za-z]+)(?:\s+and\s+[A-Z][A-Za-z]+)?\s*\((\d{4})\)"))
        {
            citations.Add(new CitationItem(match.Groups[1].Value, match.Groups[2].Value));
        }

        return citations;
    }

    private static List<CitationItem> FindReferences(string referencesText)
    {
        var references = new List<CitationItem>();
        if (string.IsNullOrWhiteSpace(referencesText))
        {
            return references;
        }

        string[] lines = referencesText
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        foreach (string line in lines)
        {
            Match match = Regex.Match(line, @"^([A-Z][A-Za-z]+).*?\((\d{4})\)");
            if (match.Success)
            {
                references.Add(new CitationItem(match.Groups[1].Value, match.Groups[2].Value));
            }
        }

        return references;
    }

    private static List<string> FindFormatIssues(string bodyText, string referencesText)
    {
        var issues = new List<string>();

        foreach (Match match in Regex.Matches(bodyText, @"\(([A-Z][A-Za-z]+)\)"))
        {
            issues.Add($"Citation missing year: {match.Value}");
        }

        foreach (Match match in Regex.Matches(bodyText, @"\(([A-Z][A-Za-z]+),\s*(\d{1,3})\)"))
        {
            issues.Add($"Citation year should have 4 digits: {match.Value}");
        }

        foreach (Match match in Regex.Matches(bodyText, @"\(([A-Z][A-Za-z]+),\s*([0-9A-Za-z]{4})\)"))
        {
            if (!Regex.IsMatch(match.Groups[2].Value, @"^\d{4}$"))
            {
                issues.Add($"Citation year is invalid: {match.Value}");
            }
        }

        foreach (Match match in Regex.Matches(bodyText, @"(?<!\()(\b[A-Z][A-Za-z]+,\s*\d{4}\b)(?!\))"))
        {
            issues.Add($"Possible parenthetical citation missing parentheses: {match.Groups[1].Value}");
        }

        if (!string.IsNullOrWhiteSpace(referencesText))
        {
            string[] lines = referencesText
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            foreach (string line in lines)
            {
                if (!Regex.IsMatch(line, @"\(\d{4}\)"))
                {
                    issues.Add($"Reference missing year in parentheses: {line}");
                }
            }
        }

        return issues.Distinct().ToList();
    }

    private static (bool HasReferencesSection, string BodyText, string ReferencesText) SplitReferencesSection(string content)
    {
        Match match = Regex.Match(
            content,
            @"(?im)^\s*(References|Reference List|Bibliography)\s*$");

        if (!match.Success)
        {
            return (false, content, string.Empty);
        }

        string bodyText = content[..match.Index];
        string referencesText = content[(match.Index + match.Length)..].Trim();

        return (true, bodyText, referencesText);
    }

    private static bool MatchesCitation(CitationItem reference, CitationItem citation)
    {
        return reference.Author.Equals(citation.Author, StringComparison.OrdinalIgnoreCase)
            && reference.Year == citation.Year;
    }

    private static string GetOverallStatus(
        bool hasReferencesSection,
        string referencesText,
        List<string> issues,
        List<string> missingReferences,
        List<string> unusedReferences)
    {
        if (!hasReferencesSection || string.IsNullOrWhiteSpace(referencesText))
        {
            return "Failed";
        }

        if (issues.Count > 0 || missingReferences.Count > 0 || unusedReferences.Count > 0)
        {
            return "Warning";
        }

        return "Passed";
    }

    private static string NormalizeText(string value)
    {
        return value.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    private static string ToMultilineText(List<string> items)
    {
        return items.Count == 0 ? string.Empty : string.Join(Environment.NewLine, items);
    }

    private record CitationItem(string Author, string Year);
}
