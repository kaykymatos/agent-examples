using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace FoundryToolkit.Workflows;

public sealed class WorkflowFactory
{
    public Workflow Create(
        AIAgent startAgent,
        Action<WorkflowBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(startAgent);
        ArgumentNullException.ThrowIfNull(configure);

        WorkflowBuilder builder = new WorkflowBuilder(startAgent);

        configure(builder);

        return builder.Build();
    }
}