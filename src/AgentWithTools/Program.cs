using AgentWithTools.Tools;
using Azure.AI.Projects;
using Azure.Identity;
using DotNetEnv;
using FoundryToolkit.Agents;
using FoundryToolkit.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

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


var reportTools = new AITool[]
{
    AIFunctionFactory.Create(ReportTools.GenerateSalesExcel, "GerarExcelVendas", "Gera uma planilha Excel com dados de vendas e retorna o caminho do arquivo."),
    AIFunctionFactory.Create(ReportTools.GenerateSummaryPdf, "GerarPdfResumo", "Gera um documento PDF resumido com um título e conteúdo fornecidos.")
};

AIAgent agent = agentFactory.Create(
    new AgentOptions
    {
        Model = deployment,
        Name = "Desenvolvedor",
        Instructions = """
        Você é um assistente de automação corporativa especializado na geração de relatórios e documentos.

        Sua função é ajudar o usuário a criar arquivos de forma prática, utilizando as ferramentas disponíveis.

        **Diretrizes:**
        - Identifique o tipo de documento solicitado e utilize a ferramenta adequada para gerá-lo.
        - Antes de gerar qualquer arquivo, se o usuário não informar o local onde o arquivo deve ser salvo salve ele na pasta C:\Users\Usuario\Downloads, caso o caminho ainda não tenha sido informado.
        - Utilize o caminho fornecido pelo usuário para salvar o arquivo no local desejado.
        - Se o usuário não souber qual pasta utilizar, solicite uma sugestão ou ofereça uma pasta padrão.
        - Após a geração, informe o nome do arquivo, o tipo de documento e o caminho completo onde foi salvo.
        - Não afirme que um arquivo foi criado ou salvo sem que a ferramenta correspondente confirme a operação.
        - Seja objetivo, cordial e solicite apenas as informações necessárias para concluir a tarefa.
        """,
        Tools = reportTools
    });

var session = await agent.CreateSessionAsync();

Console.WriteLine("Iniciando o agente...");
Console.WriteLine("Digite uma pergunta ou comando para o agente:");
await Utils.RunChatLoopWithSession(agent, session);
