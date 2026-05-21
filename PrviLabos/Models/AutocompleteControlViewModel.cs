namespace PrviLabos.Models;

public sealed class AutocompleteControlViewModel
{
    public required string Name { get; init; }
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string SearchUrl { get; init; }
    public string Placeholder { get; init; } = string.Empty;
    public int? SelectedValue { get; init; }
    public string? SelectedText { get; init; }
    public bool IsRequired { get; init; }
}

