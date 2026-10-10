namespace FoundryToolkit.Skills;

public sealed record SkillContext(
    string Input,
    IServiceProvider? Services = null
);