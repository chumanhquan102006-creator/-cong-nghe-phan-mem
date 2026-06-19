using AcademicAIAssistant.Services;
using AcademicAIAssistant.Tests.Helpers;

namespace AcademicAIAssistant.Tests.Services;

public class FileValidationServiceTests
{
    private readonly FileValidationService _service = new();

    [Fact]
    public void ValidatePdf_WithValidPdfSignature_ReturnsValid()
    {
        var file = FakeFormFileFactory.Create("paper.pdf", "application/pdf", "%PDF test"u8.ToArray());

        FileValidationResult result = _service.ValidatePdf(file);

        Assert.True(result.IsValid);
        Assert.Equal(".pdf", result.SafeExtension);
    }

    [Fact]
    public void ValidatePdf_WithFakePdfSignature_ReturnsInvalid()
    {
        var file = FakeFormFileFactory.Create("fake.pdf", "application/pdf", "hello"u8.ToArray());

        FileValidationResult result = _service.ValidatePdf(file);

        Assert.False(result.IsValid);
        Assert.Equal("Upload_InvalidPdfFile", result.ErrorMessageKey);
    }

    [Fact]
    public void ValidatePdf_WithWrongExtension_ReturnsInvalid()
    {
        var file = FakeFormFileFactory.Create("paper.docx", "application/pdf", "%PDF test"u8.ToArray());

        FileValidationResult result = _service.ValidatePdf(file);

        Assert.False(result.IsValid);
        Assert.Equal("Upload_OnlyPdfAllowed", result.ErrorMessageKey);
    }

    [Fact]
    public void ValidatePdf_WhenTooLarge_ReturnsInvalid()
    {
        byte[] content = FakeFormFileFactory.WithPadding("%PDF"u8.ToArray(), 10 * 1024 * 1024 + 1);
        var file = FakeFormFileFactory.Create("large.pdf", "application/pdf", content);

        FileValidationResult result = _service.ValidatePdf(file);

        Assert.False(result.IsValid);
        Assert.Equal("Upload_PdfTooLarge", result.ErrorMessageKey);
    }

    [Fact]
    public void ValidateImage_WithValidJpgSignature_ReturnsValid()
    {
        var file = FakeFormFileFactory.Create("scan.jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0x00 });

        FileValidationResult result = _service.ValidateImage(file);

        Assert.True(result.IsValid);
        Assert.Equal(".jpg", result.SafeExtension);
    }

    [Fact]
    public void ValidateImage_WithFakeJpgSignature_ReturnsInvalid()
    {
        var file = FakeFormFileFactory.Create("fake.jpg", "image/jpeg", "hello"u8.ToArray());

        FileValidationResult result = _service.ValidateImage(file);

        Assert.False(result.IsValid);
        Assert.Equal("Upload_InvalidImageFile", result.ErrorMessageKey);
    }

    [Fact]
    public void ValidateImage_WithValidPngSignature_ReturnsValid()
    {
        var file = FakeFormFileFactory.Create("scan.png", "image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00 });

        FileValidationResult result = _service.ValidateImage(file);

        Assert.True(result.IsValid);
        Assert.Equal(".png", result.SafeExtension);
    }
}
