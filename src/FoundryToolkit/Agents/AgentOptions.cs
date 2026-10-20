using Microsoft.Extensions.AI;

namespace FoundryToolkit.Agents;

public sealed class AgentOptions
{
    public required string Name { get; init; }

    public required string Model { get; init; }

    public required string Instructions { get; init; }

    public IList<string>? SkillPaths { get; init; }

    public IList<AITool>? Tools { get; init; }
}