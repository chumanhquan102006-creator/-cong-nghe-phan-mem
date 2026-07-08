using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace AcademicAIAssistant.Services;

public class PdfTextExtractionService
{
    public PdfTextExtractionResult ExtractText(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("PDF file was not found on the server.");
        }

        try
        {
            using var pdf = PdfDocument.Open(filePath);
            var textBuilder = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                var words = page.GetWords()
                    .Select(word => word.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text));

                textBuilder.AppendLine(string.Join(" ", words));
                textBuilder.AppendLine();
            }

            string text = NormalizeText(textBuilder.ToString());
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("No readable text was found in this PDF.");
            }

            return new PdfTextExtractionResult
            {
                Text = text,
                PageCount = pdf.NumberOfPages,
                WordCount = CountWords(text)
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Could not read text from this PDF file.", ex);
        }
    }

    internal static string NormalizeText(string value)
    {
        string text = value.Replace("\r\n", "\n").Replace("\r", "\n");

        // Keep a word boundary when PDF line breaks split ordinary reading flow.
        text = Regex.Replace(text, @"(?<=[\p{L}\p{N}])\n+(?=[\p{L}\p{N}])", " ");

        // Add missing spaces after punctuation, for example "word.Next" or "word,Next".
        text = Regex.Replace(text, @"([.,;:!?])(?=[\p{L}\p{N}])", "$1 ");

        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static int CountWords(string value)
    {
        return Regex.Matches(value, @"\b[\p{L}\p{N}']+\b").Count;
    }
}
