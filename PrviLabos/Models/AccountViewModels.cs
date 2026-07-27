using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Models;

public sealed class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB must be exactly 11 digits.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB may contain only numbers.")]
    [Display(Name = "OIB")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG must be exactly 13 digits.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG may contain only numbers.")]
    [Display(Name = "JMBG")]
    public string JMBG { get; set; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public sealed class ExternalLoginConfirmationViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB must be exactly 11 digits.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB may contain only numbers.")]
    [Display(Name = "OIB")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG must be exactly 13 digits.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG may contain only numbers.")]
    [Display(Name = "JMBG")]
    public string JMBG { get; set; } = string.Empty;
}
