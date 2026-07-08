using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace AcademicAIAssistant.Helpers;

public static class TextExportExtensions
{
    public static FileContentResult TxtFile(
        this Controller controller,
        string filePrefix,
        string content,
        DateTime createdAt)
    {
        string safePrefix = Regex.Replace(filePrefix.ToLowerInvariant(), @"[^a-z0-9-]+", "-").Trim('-');
        string fileName = $"{safePrefix}-{createdAt:yyyyMMdd-HHmm}.txt";
        byte[] bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);

        return controller.File(bytes, "text/plain; charset=utf-8", fileName);
    }
}
