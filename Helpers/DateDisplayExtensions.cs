namespace AcademicAIAssistant.Helpers;

public static class DateDisplayExtensions
{
    private const string DefaultDateTimeFormat = "dd/MM/yyyy HH:mm";

    public static string ToDisplayDateTime(this DateTime value)
    {
        return value.ToString(DefaultDateTimeFormat);
    }

    public static string ToDisplayDateTime(this DateTime? value, string fallbackText)
    {
        return value.HasValue ? value.Value.ToDisplayDateTime() : fallbackText;
    }
}
