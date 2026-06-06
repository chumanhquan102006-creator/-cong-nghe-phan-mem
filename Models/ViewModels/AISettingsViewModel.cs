using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class AISettingsViewModel
{
    public static readonly string[] ProviderOptions = { "OpenAI", "Gemini", "Custom" };

    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = "Gemini";

    [Required]
    [Display(Name = "Model")]
    [StringLength(100)]
    public string ModelName { get; set; } = "gemini-1.5-flash";

    [DataType(DataType.Password)]
    [Display(Name = "API Key")]
    public string? ApiKey { get; set; }

    public bool IsEnabled { get; set; }

    public string MaskedApiKey { get; set; } = "Not set";

    public bool HasApiKey { get; set; }
}
