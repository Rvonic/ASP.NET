using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Models;

namespace PrviLabos.Services.Validation;

public sealed class VehicleFormValidator
{
    private readonly PrviLabosDbContext _context;

    public VehicleFormValidator(PrviLabosDbContext context)
    {
        _context = context;
    }

    public Task ValidateAsync(VehicleCreateModel model, ModelStateDictionary modelState, int? vehicleId = null)
    {
        return ValidateCoreAsync(model.PlateNumber, model.CurrentLocationId, modelState, vehicleId);
    }

    public Task ValidateAsync(VehicleEditModel model, ModelStateDictionary modelState, int? vehicleId = null)
    {
        return ValidateCoreAsync(model.PlateNumber, model.CurrentLocationId, modelState, vehicleId);
    }

    private async Task ValidateCoreAsync(string plateNumber, int? currentLocationId, ModelStateDictionary modelState, int? vehicleId)
    {
        if (string.IsNullOrWhiteSpace(plateNumber))
        {
            modelState.AddModelError(nameof(VehicleCreateModel.PlateNumber), "Plate number is required.");
        }

        if (!string.IsNullOrWhiteSpace(plateNumber))
        {
            var duplicatePlateQuery = _context.Vehicles.Where(v => v.PlateNumber == plateNumber.Trim());
            if (vehicleId.HasValue)
            {
                duplicatePlateQuery = duplicatePlateQuery.Where(v => v.Id != vehicleId.Value);
            }

            if (await duplicatePlateQuery.AnyAsync())
            {
                modelState.AddModelError(nameof(VehicleCreateModel.PlateNumber), "Plate number must be unique.");
            }
        }

        if (currentLocationId.HasValue)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == currentLocationId.Value);
            if (!locationExists)
            {
                modelState.AddModelError(nameof(VehicleCreateModel.CurrentLocationId), "Selected location does not exist.");
            }
        }
    }
}