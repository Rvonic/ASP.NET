using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Models;

public sealed class CustomerEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Ime")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Prezime")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [EmailAddress(ErrorMessage = "Unesi ispravnu email adresu.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Država")]
    public string PhoneCountryCode { get; set; } = PhoneCountryCatalog.DefaultDialCode;

    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "Broj telefona mora imati točno 9 znamenki.")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "Broj telefona mora imati točno 9 znamenki.")]
    [Display(Name = "Broj telefona")]
    public string PhoneLocalNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^[A-Za-z]{2}\d{7}$", ErrorMessage = "Vozačka dozvola mora biti u formatu AA1234567.")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "Vozačka dozvola mora imati 9 znakova.")]
    [Display(Name = "Broj vozačke dozvole")]
    public string DriverLicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Datum rođenja")]
    public DateOnly? DateOfBirth { get; set; }
}