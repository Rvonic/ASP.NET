using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PrviLabos.Models;

namespace PrviLabos.Services.Validation;

public sealed class LocationFormValidator
{
    public void Validate(LocationCreateModel model, ModelStateDictionary modelState)
    {
        ValidateObject(model, modelState);
    }

    public void Validate(LocationEditModel model, ModelStateDictionary modelState)
    {
        ValidateObject(model, modelState);
    }

    private static void ValidateObject(object model, ModelStateDictionary modelState)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, true);

        foreach (var result in results)
        {
            var members = result.MemberNames.Any()
                ? result.MemberNames
                : new[] { string.Empty };

            foreach (var memberName in members)
            {
                modelState.AddModelError(memberName, result.ErrorMessage ?? "Invalid value.");
            }
        }
    }
}