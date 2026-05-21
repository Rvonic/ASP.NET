namespace PrviLabos.Models;

public sealed class DateTimeControlViewModel
{
    public string Name { get; init; } = string.Empty;

    public string? Id { get; init; }

    public string Label { get; init; } = string.Empty;

    public DateOnly? Value { get; init; }

    public bool IsRequired { get; init; }

    public bool ShowTime { get; init; }

    public string? HelpText { get; init; }

    public string? Placeholder { get; init; }

    public string? CssClass { get; init; }
}