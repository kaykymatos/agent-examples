namespace FoundryToolkit.Agents;

public sealed class AgentOptions
{
    public required string Name { get; init; }

    public required string Model { get; init; }

    public required string Instructions { get; init; }
}