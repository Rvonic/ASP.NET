using Microsoft.AspNetCore.Mvc.ModelBinding;
using PrviLabos.Models;

namespace PrviLabos.Services.Validation;

public sealed class TicketFormValidator
{
    public void Validate(TicketFormModel model, ModelStateDictionary modelState)
    {
        if (model.BookingId <= 0)
        {
            modelState.AddModelError(nameof(TicketFormModel.BookingId), "Selected booking does not exist.");
        }

        if (model.RequestedDropoffLocationId <= 0)
        {
            modelState.AddModelError(nameof(TicketFormModel.RequestedDropoffLocationId), "Selected location does not exist.");
        }

        if (model.AssignedAgentIds.Count == 0)
        {
            modelState.AddModelError(nameof(TicketFormModel.AssignedAgentIds), "Select at least one agent.");
        }
    }
}