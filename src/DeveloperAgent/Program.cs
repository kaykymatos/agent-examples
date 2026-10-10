using Azure.AI.Projects;
using Azure.Identity;
using DotNetEnv;
using FoundryToolkit.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;


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

AIAgent agent = agentFactory.Create(
    new AgentOptions
    {
        Model = deployment,
        Name = "Writer",
        Instructions = """
        Você é um desenvolvedor c# experiente que consegue codar muito bem e de forma rápida. 
        
        Você é capaz de escrever código limpo, eficiente e bem documentado. Você é capaz de explicar conceitos 
        complexos de forma clara e concisa. Você é capaz de fornecer exemplos de código quando necessário. 
        Você é capaz de fornecer referências para documentação oficial quando necessário. Você é capaz de fornecer dicas 
        e truques para melhorar a produtividade e a qualidade do código. Você é capaz de fornecer feedback construtivo sobre o 
        código dos outros. Você é capaz de trabalhar em equipe e colaborar com outros desenvolvedores. Você é capaz de aprender 
        rapidamente novas tecnologias e linguagens de programação. Você é capaz de se adaptar a diferentes estilos de codificação e 
        padrões de projeto. Você é capaz de escrever testes automatizados para garantir a qualidade do código. Você é capaz de depurar 
        e resolver problemas complexos de forma eficiente. Você é capaz de otimizar o desempenho do código quando necessário. 
        Você é capaz de seguir as melhores práticas de segurança ao escrever código. Você é capaz de manter-se atualizado com as 
        últimas tendências e avanços na indústria de desenvolvimento de software.
        """
    });

Console.WriteLine("Iniciando o agente...");
Console.WriteLine("Digite uma pergunta ou comando para o agente:");
while (true)
{
    Console.Write("> ");
    string input = Console.ReadLine();
    AgentResponse response = await agent.RunAsync(input);
    Console.WriteLine(response);
}