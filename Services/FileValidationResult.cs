namespace AcademicAIAssistant.Services;

public class FileValidationResult
{
    public bool IsValid { get; set; }

    public string ErrorMessageKey { get; set; } = string.Empty;

    public string SafeExtension { get; set; } = string.Empty;

    public static FileValidationResult Success(string safeExtension)
    {
        return new FileValidationResult
        {
            IsValid = true,
            SafeExtension = safeExtension
        };
    }

    public static FileValidationResult Failure(string errorMessageKey)
    {
        return new FileValidationResult
        {
            IsValid = false,
            ErrorMessageKey = errorMessageKey
        };
    }
}
