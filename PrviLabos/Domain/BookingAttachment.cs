using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrviLabos.Model;

public class BookingAttachment
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Booking))]
    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
