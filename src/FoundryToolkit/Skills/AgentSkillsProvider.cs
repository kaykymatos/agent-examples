namespace FoundryToolkit.Skills;

public sealed class AgentSkillsProvider
{
    private readonly Dictionary<string, string> _skills = new(StringComparer.OrdinalIgnoreCase);

    public AgentSkillsProvider(string skillsPath)
    {
        if (string.IsNullOrWhiteSpace(skillsPath))
            throw new ArgumentException("skillsPath is required", nameof(skillsPath));

        if (!Directory.Exists(skillsPath))
            return;

        foreach (string file in Directory.EnumerateFiles(skillsPath, "SKILL.md", SearchOption.AllDirectories))
        {
            try
            {
                string content = File.ReadAllText(file);
                string? name = ParseFrontMatterName(content) ?? Path.GetFileName(Path.GetDirectoryName(file) ?? file);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                string body = ParsePromptBody(content);

                if (!string.IsNullOrWhiteSpace(body) && !_skills.ContainsKey(name))
                {
                    _skills[name] = body.Trim();
                }
            }
            catch
            {
                // ignore malformed skill files
            }
        }
    }

    public IEnumerable<string> GetAvailableSkillNames() => _skills.Keys.ToArray();

    public bool TryGetSkillInstruction(string name, out string instruction)
    {
        instruction = string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return _skills.TryGetValue(name, out instruction!);
    }

    private static string? ParseFrontMatterName(string content)
    {
        if (string.IsNullOrEmpty(content))
            return null;

        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimStart();
            if (line.StartsWith("name:", StringComparison.OrdinalIgnoreCase))
            {
                int idx = line.IndexOf(':');
                if (idx >= 0 && idx + 1 < line.Length)
                {
                    string value = line.Substring(idx + 1).Trim();
                    return string.IsNullOrEmpty(value) ? null : value;
                }

                return null;
            }
        }

        return null;
    }

    private static string ParsePromptBody(string content)
    {
        return content.Trim();
    }
}
