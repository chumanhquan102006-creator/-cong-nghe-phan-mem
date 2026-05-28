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
                textBuilder.AppendLine(page.Text);
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

    private static string NormalizeText(string value)
    {
        string text = value.Replace("\r\n", "\n").Replace("\r", "\n");

        // Keep a space when PDF line breaks join two words together.
        text = Regex.Replace(text, @"(?<=[\p{Ll}\p{N}])\n(?=[\p{Lu}])", " ");
        text = Regex.Replace(text, @"(?<=[\p{L}\p{N}])\n(?=[\p{L}\p{N}])", " ");

        // Add missing spaces after punctuation, for example "word.Next" or "word,Next".
        text = Regex.Replace(text, @"([.,;:!?])(?=[\p{L}\p{N}])", "$1 ");

        // Add spaces in common PDF extraction joins like "UniversityAbstract".
        text = Regex.Replace(text, @"(?<=[\p{Ll}])(?=[\p{Lu}])", " ");

        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static int CountWords(string value)
    {
        return Regex.Matches(value, @"\b[\p{L}\p{N}']+\b").Count;
    }
}
