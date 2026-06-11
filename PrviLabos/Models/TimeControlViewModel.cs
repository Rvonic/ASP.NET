namespace PrviLabos.Models;

public sealed class TimeControlViewModel
{
    public string Name { get; init; } = string.Empty;

    public string? Id { get; init; }

    public string Label { get; init; } = string.Empty;

    public TimeOnly? Value { get; init; }

    public bool IsRequired { get; init; }

    public string? HelpText { get; init; }

    public string? CssClass { get; init; }
}
