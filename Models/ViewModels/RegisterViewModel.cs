using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Validation_FullNameRequired")]
    [StringLength(100, ErrorMessage = "Validation_FullNameMaxLength")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_EmailRequired")]
    [EmailAddress(ErrorMessage = "Validation_EmailInvalid")]
    [StringLength(150, ErrorMessage = "Validation_EmailMaxLength")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_PasswordRequired")]
    [MinLength(8, ErrorMessage = "Validation_PasswordMinLength")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_ConfirmPasswordRequired")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Validation_PasswordsDoNotMatch")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
