using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Models;

public sealed class PhoneLocalNumberAttribute : ValidationAttribute
{
    public int MinimumDigits { get; init; } = 4;

    public int MaximumDigits { get; init; } = 14;

    public PhoneLocalNumberAttribute()
    {
        ErrorMessage = "Broj može sadržavati samo znamenke i razmake.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        var digitCount = 0;
        foreach (var character in text)
        {
            if (char.IsDigit(character))
            {
                digitCount++;
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            return false;
        }

        return digitCount >= MinimumDigits && digitCount <= MaximumDigits;
    }
}