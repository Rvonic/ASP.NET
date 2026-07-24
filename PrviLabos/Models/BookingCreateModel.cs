using Microsoft.AspNetCore.Http;

namespace PrviLabos.Models;

public sealed class BookingCreateModel : BookingFormModel
{
    public List<IFormFile> Attachments { get; set; } = new();
}
