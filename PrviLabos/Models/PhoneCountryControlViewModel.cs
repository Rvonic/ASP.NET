namespace PrviLabos.Models;

public sealed class PhoneCountryControlViewModel
{
    public string Name { get; init; } = string.Empty;

    public string? Id { get; init; }

    public string Label { get; init; } = string.Empty;

    public string Value { get; init; } = PhoneCountryCatalog.DefaultDialCode;
}
