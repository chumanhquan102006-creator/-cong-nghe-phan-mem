namespace AcademicAIAssistant.Services;

public interface IFileValidationService
{
    FileValidationResult ValidatePdf(IFormFile file);

    FileValidationResult ValidateImage(IFormFile file);
}
