using AcademicAIAssistant.Models;

namespace AcademicAIAssistant.Services;

public class ReferenceGeneratorService
{
    public ReferenceItem GenerateAllCitationFormats(ReferenceItem item)
    {
        item.ApaInTextCitation = GenerateApaInText(item);
        item.ApaReference = GenerateApaReference(item);
        item.MlaInTextCitation = GenerateMlaInText(item);
        item.MlaReference = GenerateMlaReference(item);
        return item;
    }

    public string GenerateApaInText(ReferenceItem item)
    {
        string year = GetYear(item);
        string shortAuthor = GetShortAuthor(item.Author);

        if (!string.IsNullOrWhiteSpace(shortAuthor))
        {
            return $"({shortAuthor}, {year})";
        }

        return $"(\"{ShortTitle(item.Title)}\", {year})";
    }

    public string GenerateApaReference(ReferenceItem item)
    {
        string author = GetApaAuthor(item);
        string year = GetYear(item);
        string title = EnsurePeriod(item.Title);

        return item.SourceType switch
        {
            "JournalArticle" => JoinParts(
                $"{author} ({year}).",
                title,
                FormatJournalApa(item),
                FormatDoiOrUrl(item)),
            "Website" => JoinParts(
                $"{author} ({year}).",
                title,
                EnsurePeriod(item.WebsiteName),
                item.Url),
            _ => JoinParts(
                $"{author} ({year}).",
                title,
                EnsurePeriod(item.JournalOrPublisher))
        };
    }

    public string GenerateMlaInText(ReferenceItem item)
    {
        string shortAuthor = GetFirstAuthorLastName(item.Author);
        return string.IsNullOrWhiteSpace(shortAuthor)
            ? $"(\"{ShortTitle(item.Title)}\")"
            : $"({shortAuthor})";
    }

    public string GenerateMlaReference(ReferenceItem item)
    {
        string author = string.IsNullOrWhiteSpace(item.Author) ? "Unknown author." : EnsurePeriod(item.Author);
        string title = $"\"{item.Title}.\"";

        return item.SourceType switch
        {
            "JournalArticle" => JoinParts(
                author,
                title,
                FormatJournalMla(item)),
            "Website" => JoinParts(
                author,
                title,
                EnsurePeriod(item.WebsiteName),
                FormatYearComma(item.Year),
                EnsurePeriod(item.Url),
                item.AccessDate.HasValue ? $"Accessed {item.AccessDate.Value:dd MMM yyyy}." : string.Empty),
            _ => JoinParts(
                author,
                EnsurePeriod(item.Title),
                FormatPublisherYearMla(item))
        };
    }

    private static string GetApaAuthor(ReferenceItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Author))
        {
            return string.IsNullOrWhiteSpace(item.Title) ? "Unknown author." : $"{item.Title}.";
        }

        return EnsurePeriod(item.Author);
    }

    private static string GetShortAuthor(string author)
    {
        var names = SplitAuthors(author);
        if (names.Count == 0)
        {
            return string.Empty;
        }

        if (names.Count == 1)
        {
            return GetFirstAuthorLastName(names[0]);
        }

        if (names.Count == 2)
        {
            return $"{GetFirstAuthorLastName(names[0])} & {GetFirstAuthorLastName(names[1])}";
        }

        return $"{GetFirstAuthorLastName(names[0])} et al.";
    }

    private static string GetFirstAuthorLastName(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return string.Empty;
        }

        string firstAuthor = SplitAuthors(author).FirstOrDefault() ?? author;
        string trimmed = firstAuthor.Trim();

        if (trimmed.Contains(','))
        {
            return trimmed.Split(',')[0].Trim();
        }

        string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? string.Empty : parts[^1];
    }

    private static List<string> SplitAuthors(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return new List<string>();
        }

        return author
            .Replace(" and ", ",", StringComparison.OrdinalIgnoreCase)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();
    }

    private static string FormatJournalApa(ReferenceItem item)
    {
        string journal = item.JournalOrPublisher;
        string volumeIssue = string.Empty;

        if (!string.IsNullOrWhiteSpace(item.Volume))
        {
            volumeIssue = item.Volume;
            if (!string.IsNullOrWhiteSpace(item.Issue))
            {
                volumeIssue += $"({item.Issue})";
            }
        }

        string pages = string.IsNullOrWhiteSpace(item.Pages) ? string.Empty : item.Pages;
        return EnsurePeriod(JoinParts(journal, volumeIssue, pages));
    }

    private static string FormatJournalMla(ReferenceItem item)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.JournalOrPublisher))
        {
            parts.Add(item.JournalOrPublisher);
        }

        if (!string.IsNullOrWhiteSpace(item.Volume))
        {
            parts.Add($"vol. {item.Volume}");
        }

        if (!string.IsNullOrWhiteSpace(item.Issue))
        {
            parts.Add($"no. {item.Issue}");
        }

        if (!string.IsNullOrWhiteSpace(item.Year))
        {
            parts.Add(item.Year);
        }

        if (!string.IsNullOrWhiteSpace(item.Pages))
        {
            parts.Add($"pp. {item.Pages}");
        }

        return EnsurePeriod(string.Join(", ", parts));
    }

    private static string FormatPublisherYearMla(ReferenceItem item)
    {
        if (string.IsNullOrWhiteSpace(item.JournalOrPublisher) && string.IsNullOrWhiteSpace(item.Year))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(item.JournalOrPublisher))
        {
            return EnsurePeriod(item.Year);
        }

        if (string.IsNullOrWhiteSpace(item.Year))
        {
            return EnsurePeriod(item.JournalOrPublisher);
        }

        return $"{item.JournalOrPublisher}, {item.Year}.";
    }

    private static string FormatDoiOrUrl(ReferenceItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Doi))
        {
            return item.Doi.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? item.Doi
                : $"https://doi.org/{item.Doi}";
        }

        return item.Url;
    }

    private static string GetYear(ReferenceItem item)
    {
        return string.IsNullOrWhiteSpace(item.Year) ? "n.d." : item.Year.Trim();
    }

    private static string ShortTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Untitled";
        }

        string[] words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length <= 4 ? title.Trim() : string.Join(" ", words.Take(4));
    }

    private static string FormatYearComma(string year)
    {
        return string.IsNullOrWhiteSpace(year) ? string.Empty : $"{year},";
    }

    private static string EnsurePeriod(string value)
    {
        string text = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return text.EndsWith('.') ? text : text + ".";
    }

    private static string JoinParts(params string[] parts)
    {
        return string.Join(" ", parts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim()));
    }
}
