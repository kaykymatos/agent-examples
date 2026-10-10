using Azure.AI.Projects;
using Microsoft.Agents.AI;

namespace FoundryToolkit.Agents;

public sealed class AgentFactory
{
    private readonly AIProjectClient _projectClient;

    public AgentFactory(AIProjectClient projectClient)
    {
        _projectClient = projectClient;
    }

    public AIAgent Create(AgentOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ArgumentException.ThrowIfNullOrWhiteSpace(options.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Model);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Instructions);

        return _projectClient.AsAIAgent(
            model: options.Model,
            name: options.Name,
            instructions: options.Instructions
        );
    }
}