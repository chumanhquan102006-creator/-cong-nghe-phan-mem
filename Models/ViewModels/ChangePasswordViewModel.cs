using System.ComponentModel.DataAnnotations;

namespace AcademicAIAssistant.Models.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "CurrentPasswordRequired")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "NewPasswordRequired")]
    [MinLength(8, ErrorMessage = "NewPasswordMinLength")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmPasswordRequired")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "ChangePassword_PasswordsDoNotMatch")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
