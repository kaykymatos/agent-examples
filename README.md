# Exemplos de agentes com Microsoft Foundry e .NET

Este repositório reúne exemplos de aplicações de console em C# que usam modelos implantados no Microsoft Foundry e o Microsoft Agent Framework. Os projetos demonstram agentes com instruções próprias, conversas com histórico, skills e fluxos de trabalho com mais de um agente.

## Projetos

| Projeto | O que é |
| --- | --- |
| [`DeveloperAgent`](src/DeveloperAgent/DeveloperAgent.csproj) | Um agente de desenvolvimento C# que responde em português e mantém o contexto da conversa. |
| [`AgentSkills`](src/AgentSkills/AgentSkills.csproj) | Um agente conversacional que recebe skills descritas em arquivos `SKILL.md`; inclui exemplos de persona e transformação de texto. |
| [`AgentWithTools`](src/AgentWithTools/AgentWithTools.csproj) | Um agente que usa ferramentas para gerar planilhas Excel e relatórios PDF. |
| [`AgentesFlow`](src/AgentesFlow/AgentesFlow.csproj) | Um fluxo de três agentes: um escritor cria uma frase, um revisor aprova ou pede correções e um publicador simula a publicação. |
| [`FoundryToolkit`](src/FoundryToolkit/FoundryToolkit.csproj) | Biblioteca compartilhada pelos exemplos, com fábricas de agentes e workflows, suporte a skills e utilitário de conversa com sessão. Não é uma aplicação executável. |

## Requisitos

- .NET SDK 10.
- Uma assinatura Azure e um projeto no [Microsoft Foundry](https://ai.azure.com/).
- Um modelo implantado nesse projeto.
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli), autenticado com uma conta que tenha acesso ao projeto e ao modelo.

## Configuração

Todos os aplicativos precisam do endpoint do projeto Foundry e do nome da implantação do modelo. O código lê estas configurações:

| Chave | Valor |
| --- | --- |
| `FOUNDRY_PROJECT_ENDPOINT` | Endpoint do projeto no Microsoft Foundry — não a URL da página do portal nem a URL do catálogo do modelo. |
| `AZURE_AI_MODEL_DEPLOYMENT_NAME` | Nome da implantação do modelo no projeto Foundry. |

Autentique-se com a Azure CLI e, se necessário, selecione a assinatura correta:

```powershell
az login
az account set --subscription "<ID-DA-SUBSCRICAO>"
```

### Usando variáveis de ambiente

No PowerShell, configure as variáveis na mesma janela em que executará o exemplo:

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<ENDPOINT-DO-PROJETO>"
$env:AZURE_AI_MODEL_DEPLOYMENT_NAME = "<NOME-DA-IMPLANTACAO>"
```

### Usando .NET User Secrets

Os projetos de aplicação já têm um `UserSecretsId`. A partir da raiz do repositório, configure os valores para um dos projetos:

```powershell
dotnet user-secrets set "FOUNDRY_PROJECT_ENDPOINT" "https://<ENDPOINT-DO-PROJETO>" --project src\DeveloperAgent\DeveloperAgent.csproj
dotnet user-secrets set "AZURE_AI_MODEL_DEPLOYMENT_NAME" "<NOME-DA-IMPLANTACAO>" --project src\DeveloperAgent\DeveloperAgent.csproj
```

Os quatro aplicativos compartilham o mesmo `UserSecretsId`, portanto esses valores também serão usados por `AgentSkills`, `AgentWithTools` e `AgentesFlow`. User Secrets são apropriados para desenvolvimento local; não são criptografados e não devem ser tratados como um cofre de produção.

### Usando um arquivo `.env`

Também é possível criar um arquivo `.env` na raiz do repositório:

```dotenv
FOUNDRY_PROJECT_ENDPOINT=https://<ENDPOINT-DO-PROJETO>
AZURE_AI_MODEL_DEPLOYMENT_NAME=<NOME-DA-IMPLANTACAO>
```

Os aplicativos procuram esse arquivo ao percorrer os diretórios acima da aplicação. O `.gitignore` já ignora arquivos `.env`; não remova essa proteção nem versione configurações privadas. Como User Secrets são adicionados depois das variáveis de ambiente na configuração .NET, eles prevalecem quando a mesma chave está definida em ambos.

## Como executar

Execute os comandos a partir da raiz do repositório. Restaure as dependências uma vez:

```powershell
dotnet restore AgentExamples.slnx
```

Depois escolha um projeto:

### `DeveloperAgent` — agente desenvolvedor

```powershell
dotnet run --project src\DeveloperAgent\DeveloperAgent.csproj
```

Converse no terminal com um agente configurado para atuar como desenvolvedor C# sênior. Ele responde em português, explica decisões técnicas e ajuda a identificar e corrigir erros. A conversa mantém uma sessão; digite `/new` para iniciar outra sessão.

### `AgentSkills` — agente com skills

```powershell
dotnet run --project src\AgentSkills\AgentSkills.csproj
```

Este exemplo cria um agente conversacional com as instruções e skills encontradas no diretório `src\AgentSkills\Skills`. A skill incluída, `speak-like-a-pirate`, define uma persona de pirata amigável para crianças. Peça ao agente para usar essa persona ou faça outras perguntas.

As skills disponíveis neste exemplo são:

| Skill | Descrição |
| --- | --- |
| `speak-like-a-pirate` | Responde com a persona de um pirata amigável para crianças, preservando o sentido do texto. |
| `uppercase-formatter` | Converte o texto para maiúsculas e adiciona o prefixo `[PROCESSADO]:`. |

Para adicionar uma skill, crie um subdiretório dentro de `src\AgentSkills\Skills` com um arquivo `SKILL.md`. O arquivo deve começar com metadados `name` e `description` em YAML e descrever no corpo as instruções da skill:

```markdown
---
name: nome-da-skill
description: Resumo de quando e como usar a skill.
---

# Instruções

Descreva aqui o comportamento esperado.
```

O agente recebe as skills encontradas no diretório configurado pelo exemplo. Você pode pedir, por exemplo, para reescrever uma frase como pirata ou formatar um texto em maiúsculas.

### `AgentWithTools` — agente com ferramentas de geração de documentos

```powershell
dotnet run --project src\AgentWithTools\AgentWithTools.csproj
```

Converse com o agente para solicitar uma planilha Excel ou um relatório PDF. Ele pode gerar uma planilha com título, colunas, dados, filtros e opções de formatação; o PDF pode incluir parágrafos e uma tabela opcional. Os arquivos são salvos na pasta `Downloads` do usuário atual. O projeto usa ClosedXML para Excel e QuestPDF para PDF; o QuestPDF está configurado com a licença Community.

### `AgentesFlow` — fluxo de agentes

```powershell
dotnet run --project src\AgentesFlow\AgentesFlow.csproj
```

Digite um tema quando solicitado. O fluxo então:

1. Pede ao `Writer` uma frase sobre o tema.
2. Envia a frase ao `Reviewer`, que a aprova ou pede uma correção.
3. Em caso de rejeição, devolve o feedback ao `Writer` e repete a revisão.
4. Após a aprovação, envia a frase ao `Publisher`, que simula sua publicação.

O resultado de cada agente é exibido no terminal. Pressione `Ctrl+C` para interromper os aplicativos interativos.

## Biblioteca compartilhada: `FoundryToolkit`

Os exemplos usam essa biblioteca para compartilhar componentes:

- `AgentFactory` cria agentes ligados ao projeto Foundry, com modelo, nome, instruções e, opcionalmente, diretórios de skills.
- `WorkflowFactory` cria workflows do Microsoft Agent Framework.
- `Utils.RunChatLoopWithSession` executa o loop de conversa e permite iniciar uma nova sessão com `/new`.
- O namespace `FoundryToolkit.Skills` contém contratos e estruturas para registrar e executar skills (`ISkill`, `SkillContext` e `SkillRegistry`).

## Solução

Os cinco projetos estão incluídos em [`AgentExamples.slnx`](AgentExamples.slnx). Para compilar a solução sem executar uma aplicação:

```powershell
dotnet build AgentExamples.slnx
```

## Problemas comuns

- **Endpoint ou modelo ausente:** confira os nomes das chaves e os valores configurados. User Secrets podem prevalecer sobre variáveis de ambiente.
- **Falha de autenticação ou acesso negado:** execute `az login` e confirme que a conta tem acesso ao projeto Foundry e à implantação.
- **Implantação não encontrada:** use o nome exato da implantação do modelo, que pode ser diferente do nome do modelo no catálogo.
- **Endpoint inválido:** copie o endpoint do projeto Foundry, não a URL do portal nem a página do modelo.
