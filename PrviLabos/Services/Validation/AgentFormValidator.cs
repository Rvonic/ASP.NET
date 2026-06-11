using Microsoft.AspNetCore.Mvc.ModelBinding;
using PrviLabos.Models;

namespace PrviLabos.Services.Validation;

public sealed class AgentFormValidator
{
    public void Validate(AgentFormModel model, ModelStateDictionary modelState)
    {
        if (model.ShiftStart.HasValue && model.ShiftEnd.HasValue && model.ShiftEnd <= model.ShiftStart)
        {
            modelState.AddModelError(nameof(AgentFormModel.ShiftEnd), "Kraj smjene mora biti nakon početka smjene.");
        }
    }
}