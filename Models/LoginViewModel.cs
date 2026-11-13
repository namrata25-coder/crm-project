using System.ComponentModel.DataAnnotations;

namespace CrmLeadManagement.Models;

/// <summary>
/// View model for the web login page.
/// Supports login via User ID or Email address.
/// </summary>
public class LoginViewModel
{
    [Display(Name = "User ID")]
    public string? UserId { get; set; } = "";

    [Display(Name = "Email")]
    public string? Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; } = true;

    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    /// <summary>Target authentication portal: 'admin' for Admin Login, 'user' for User Login.</summary>
    public string? LoginType { get; set; } = "user";

    /// <summary>
    /// Returns the effective credential input (User ID, Email, or Username), trimmed.
    /// </summary>
    public string GetEffectiveInput()
    {
        if (!string.IsNullOrWhiteSpace(UserId)) return UserId.Trim();
        if (!string.IsNullOrWhiteSpace(Email)) return Email.Trim();
        return "";
    }
}
