namespace FoundryToolkit.Skills;

public interface ISkill
{
    string Name { get; }

    Task<object?> ExecuteAsync(
        SkillContext context,
        CancellationToken cancellationToken = default
    );
}