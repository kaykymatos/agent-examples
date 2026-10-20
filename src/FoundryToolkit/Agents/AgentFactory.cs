using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

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

        List<AIContextProvider> skills = new List<AIContextProvider>();
        if (options.SkillPaths is not null && options.SkillPaths.Count > 0)
            foreach (string item in options.SkillPaths)
                skills.Add(new AgentSkillsProvider(item, options: new AgentSkillsProviderOptions
                {
                    DisableReadSkillResourceApproval = true,
                    DisableRunSkillScriptApproval = true,
                    DisableLoadSkillApproval = true,
                }));

        return _projectClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Name = options.Name,

                ChatOptions = new()
                {
                    ModelId = options.Model,
                    Instructions = options.Instructions,
                    Tools = options?.Tools,
                },
                AIContextProviders = skills,

            }
        );
    }
}