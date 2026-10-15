using Azure.AI.Projects;
using Azure.Identity;
using DotNetEnv;
using FoundryToolkit.Agents;
using FoundryToolkit.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
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

AIAgent writer = agentFactory.Create(
    new AgentOptions
    {
        Model = deployment,
        Name = "Writer",
        Instructions = """
            Você é um escritor especializado em programação C#.

            Escreva uma frase sobre o tema recebido.
            Se receber uma mensagem iniciada por REJECTED,
            corrija a frase conforme o feedback do Reviewer.

            Retorne somente a frase, sem explicações.
            """
    });

AIAgent reviewer = agentFactory.Create(
    new AgentOptions
    {
        Model = deployment,
        Name = "Reviewer",
        Instructions = """
            Você é um revisor de frases sobre programação C#.

               Analise a frase recebida.

               Se estiver correta, responda exatamente neste formato:
               APPROVED
               FRASE: <frase original completa>

               Se estiver incorreta, responda neste formato:
               REJECTED: <explique brevemente o problema e como corrigir>

               Não reescreva a frase quando estiver aprovada.
               Não use APPROVED quando houver problemas.
            """
    });

AIAgent publisher = agentFactory.Create(
    new AgentOptions
    {
        Model = deployment,
        Name = "Publisher",
        Instructions = """
            Você é um publicador.

            Receberá a resposta do Reviewer contendo o status
            APPROVED e a frase aprovada.

            Extraia somente o conteúdo depois de "FRASE:"
            e simule a publicação dessa frase.

            Responda no formato:
            PUBLISHED: <frase aprovada>
            """
    });

WorkflowFactory workflowFactory = new WorkflowFactory();

bool reviewerApproved = false;
Workflow workflow = workflowFactory.Create(
    writer,
    builder =>
    {
        builder.AddEdge(writer, reviewer);

        builder.AddEdge<object>(
            reviewer,
            writer,
            condition: message =>
            {
                if (message is TurnToken)
                    return reviewerApproved == false;

                if (message is not List<ChatMessage> messages)
                    return false;

                ChatMessage? reviewerMessage = messages.LastOrDefault(m => m.AuthorName == "Reviewer");

                string? text = reviewerMessage?.Text?.Trim();
                var rejected = text?.Contains("REJECTED", StringComparison.OrdinalIgnoreCase) == true;
                if (rejected)
                    reviewerApproved = false;
                return rejected;

            });

        builder.AddEdge<object>(
             reviewer,
             publisher,
             condition: message =>
             {
                 if (message is TurnToken)
                     return reviewerApproved == true;

                 if (message is not List<ChatMessage> messages)
                     return false;

                 ChatMessage? reviewerMessage = messages.LastOrDefault(m => m.AuthorName == "Reviewer");
                 
                 string? text = reviewerMessage?.Text?.Trim();
                 var approved = text?.Contains("APPROVED", StringComparison.OrdinalIgnoreCase) == true;
                 if (approved)
                     reviewerApproved = true;
                 return approved;
             });

        builder.WithOutputFrom(publisher);
    });


Console.WriteLine("=================================");
Console.WriteLine(" Writer -> Reviewer -> Publisher");
Console.WriteLine("=================================");
Console.WriteLine();

Console.Write("Digite o tema: ");

string? input = Console.ReadLine();

if (string.IsNullOrWhiteSpace(input))
    return;

await using StreamingRun run = await InProcessExecution.RunStreamingAsync(
        workflow,
        new ChatMessage(ChatRole.User, input));

await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
string currentAgent = string.Empty;

await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent outputEvent)
    {
        if (outputEvent.ExecutorId != currentAgent)
        {
            currentAgent = outputEvent.ExecutorId;
            Console.WriteLine($"\n[{currentAgent}]: ");
        }
        Console.Write(outputEvent.Data);
    }
}