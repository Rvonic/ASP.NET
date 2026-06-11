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
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Ime")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Prezime")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB mora imati tocno 11 znamenki.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadrzavati samo brojeve.")]
    [Display(Name = "OIB")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tocno 13 znamenki.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadrzavati samo brojeve.")]
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
    [Display(Name = "Ime")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Prezime")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB mora imati tocno 11 znamenki.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadrzavati samo brojeve.")]
    [Display(Name = "OIB")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tocno 13 znamenki.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadrzavati samo brojeve.")]
    [Display(Name = "JMBG")]
    public string JMBG { get; set; } = string.Empty;
}
