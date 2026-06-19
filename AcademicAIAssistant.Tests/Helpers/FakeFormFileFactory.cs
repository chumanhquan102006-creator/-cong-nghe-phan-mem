using Microsoft.AspNetCore.Http;

namespace AcademicAIAssistant.Tests.Helpers;

public static class FakeFormFileFactory
{
    public static IFormFile Create(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    public static byte[] WithPadding(byte[] header, int totalLength)
    {
        byte[] content = new byte[totalLength];
        Array.Copy(header, content, header.Length);
        return content;
    }
}
