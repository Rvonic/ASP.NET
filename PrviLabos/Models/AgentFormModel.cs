using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Models;

public abstract class AgentFormModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Ime i prezime")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Tim")]
    public string TeamName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Početak smjene")]
    public DateTime? ShiftStart { get; set; }

    [Required]
    [Display(Name = "Kraj smjene")]
    public DateTime? ShiftEnd { get; set; }

    [Display(Name = "Na dužnosti")]
    public bool IsOnDuty { get; set; }
}