using DSharpPlus.CommandsNext;

namespace House.House.Services.Fuzzy;

public readonly struct HouseFuzzyResult
{
    public int Distance { get; init; }
    public string Icon { get; init; }
    public string Similarity { get; init; }
    public int Percentage { get; init; }

    public Command Command { get; init; }
    public string? ModuleName { get; init; }

    public override string ToString() => $"{Icon} `{Command.Name}` ({Similarity}, {Percentage}%) from {ModuleName ?? "unknown module"}";
}