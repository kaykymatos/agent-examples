namespace FoundryToolkit.Skills;

public sealed class SkillRegistry
{
    private readonly Dictionary<string, ISkill> _skills =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ISkill skill)
    {
        ArgumentNullException.ThrowIfNull(skill);

        ArgumentException.ThrowIfNullOrWhiteSpace(skill.Name);

        if (!_skills.TryAdd(skill.Name, skill))
        {
            throw new InvalidOperationException(
                $"A skill '{skill.Name}' já está registrada."
            );
        }
    }

    public ISkill GetRequired(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_skills.TryGetValue(name, out ISkill? skill))
        {
            throw new KeyNotFoundException(
                $"A skill '{name}' não foi encontrada."
            );
        }

        return skill;
    }

    public async Task<object?> ExecuteAsync(
        string name,
        SkillContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        ISkill skill = GetRequired(name);

        return await skill.ExecuteAsync(
            context,
            cancellationToken
        );
    }

    public IReadOnlyCollection<string> GetRegisteredNames()
    {
        return _skills.Keys.ToArray();
    }
}