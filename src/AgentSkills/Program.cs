using Azure.AI.Projects;
using Azure.Identity;
using DotNetEnv;
using FoundryToolkit.Agents;
using FoundryToolkit.Shared;
using FoundryToolkit.Skills;
using Google.Protobuf;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.IO;


Env.NoClobber().TraversePath().Load();

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true)
    .Build();

Uri projectEndpoint = new Uri(configuration["FOUNDRY_PROJECT_ENDPOINT"]!);
string deployment = configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"]!;

AgentFactory agentFactory = new AgentFactory(
    new AIProjectClient(
    projectEndpoint,
    new AzureCliCredential()));

string skillsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AgentSkills", "Skills"));

var skillsProvider = new FoundryToolkit.Skills.AgentSkillsProvider(skillsPath);

AgentOptions options = new AgentOptions
{
    Model = deployment,
    Name = "Desenvolvedor",
    Instructions = """
        Você é um desenvolvedor C# Sênior, especialista em .NET e boas práticas de desenvolvimento.

        Escreva código limpo, seguro, eficiente e de fácil manutenção.
        Siga os princípios SOLID, Clean Code e Clean Architecture quando apropriados.
        Utilize boas práticas de orientação a objetos, tratamento de erros e testes automatizados.
        Prefira soluções simples e evite complexidade desnecessária.
        Explique suas decisões técnicas de forma clara e objetiva.
        Ao identificar erros, explique a causa e apresente uma solução.
        Não invente APIs ou funcionalidades inexistentes.
        Responda em português brasileiro.
        """,
    SkillPaths = [skillsPath]
};

AIAgent agent = agentFactory.Create(options);

Console.WriteLine("Iniciando o agente...");
Console.WriteLine("Digite uma pergunta ou comando para o agente:");

AgentSession session = await agent.CreateSessionAsync();
await Utils.RunChatLoopWithSession(agent, session);