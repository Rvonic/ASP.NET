using System.ComponentModel.DataAnnotations;
using PrviLabos.Model;

namespace PrviLabos.Models;

public abstract class TicketFormModel
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Broj prijave")]
    public string TicketNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Rezervacija")]
    public int BookingId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Tražena lokacija iskrcaja")]
    public int RequestedDropoffLocationId { get; set; }

    [Required]
    [StringLength(4000)]
    [Display(Name = "Opis")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Prioritet")]
    public TicketPriority Priority { get; set; }

    [Required]
    [Display(Name = "Status")]
    public TicketStatus Status { get; set; }

    [Display(Name = "Dodijeljeni agenti")]
    public List<int> AssignedAgentIds { get; set; } = new();

    [Display(Name = "Riješeno u")]
    public DateTime? ResolvedAt { get; set; }
}