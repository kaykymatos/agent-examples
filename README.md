# Microsoft Foundry Agent with C#

A simple C# console application that creates an AI agent using Microsoft Foundry and the Microsoft Agent Framework.

The agent receives questions in the console and generates responses based on its configured instructions.

## Prerequisites

Before running the project, make sure you have:

- The .NET SDK version required by the project.
- An Azure subscription and access to a Microsoft Foundry project.
- A deployed model in your Foundry project.
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) installed and signed in.
- The project source code cloned to your machine.

## 1. Sign in to Azure

Because the application uses `AzureCliCredential`, authenticate through Azure CLI:

```bash
az login
```

If you have access to multiple subscriptions, select the one containing your Foundry project:

```bash
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
```

Make sure your account has the permissions required to access the Foundry project and its deployed model.

## 2. Get your Microsoft Foundry project endpoint

1. Open [Microsoft Foundry](https://ai.azure.com/).
2. Open the project you want to use.
3. Find the project's endpoint in its project details or connection information.
4. Copy the project endpoint URL.

Use the **project endpoint**, not the model's public documentation URL. The exact location of the endpoint in the portal may vary as the interface evolves.

## 3. Get your model deployment name

In your Foundry project, open the model deployments page and copy the name of the deployed model you want to use.

Use the **deployment name** configured in your project. It may differ from the model's catalog name.

## 4. Configure the application

The application reads these settings:

| Setting | Description |
| --- | --- |
| `FOUNDRY_PROJECT_ENDPOINT` | The endpoint URL of your Microsoft Foundry project. |
| `AZURE_AI_MODEL_DEPLOYMENT_NAME` | The name of your deployed model. |

You can provide them through environment variables or .NET User Secrets.

### Option A: Environment variables

Set the variables in your terminal before running the application.

**PowerShell (Windows):**

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<YOUR-FOUNDRY-PROJECT-ENDPOINT>"
$env:AZURE_AI_MODEL_DEPLOYMENT_NAME = "<YOUR-MODEL-DEPLOYMENT-NAME>"
dotnet run
```

Replace both placeholder values with your actual project endpoint and deployment name.

These commands set the variables only for the current PowerShell session.

**Bash (Linux/macOS):**

```bash
export FOUNDRY_PROJECT_ENDPOINT="https://<YOUR-FOUNDRY-PROJECT-ENDPOINT>"
export AZURE_AI_MODEL_DEPLOYMENT_NAME="<YOUR-MODEL-DEPLOYMENT-NAME>"
dotnet run
```

### Option B: .NET User Secrets (recommended for local development)

The application calls `AddUserSecrets<Program>(optional: true)`, so you can store these settings in User Secrets instead of adding them to source control.

Run these commands from the directory containing the application project's `.csproj` file:

```bash
dotnet user-secrets init
dotnet user-secrets set "FOUNDRY_PROJECT_ENDPOINT" "https://<YOUR-FOUNDRY-PROJECT-ENDPOINT>"
dotnet user-secrets set "AZURE_AI_MODEL_DEPLOYMENT_NAME" "<YOUR-MODEL-DEPLOYMENT-NAME>"
```

Replace the placeholder values with your actual settings. User Secrets are intended for local development and are not encrypted; do not treat them as a production secret store.

### Option C: `.env` file

The code calls `DotNetEnv` to load environment variables from a `.env` file. If you prefer this option, create a `.env` file in a location that `DotNetEnv` can discover:

```dotenv
FOUNDRY_PROJECT_ENDPOINT=https://<YOUR-FOUNDRY-PROJECT-ENDPOINT>
AZURE_AI_MODEL_DEPLOYMENT_NAME=<YOUR-MODEL-DEPLOYMENT-NAME>
```

Do not commit `.env` files containing private configuration or credentials. Add `.env` to `.gitignore`.

> **Configuration precedence:** the code uses `AddEnvironmentVariables()` before `AddUserSecrets<Program>()`. With the default .NET configuration providers, later providers override earlier providers, so User Secrets override environment variables when both define the same key. The `.env` loader also populates environment variables; behavior can depend on whether those variables already exist and on the `DotNetEnv` loading options.

## 5. Run the application

From the directory containing the console application's `.csproj` file:

```bash
dotnet restore
dotnet run
```

When the application starts, enter a question or instruction in the console. The agent will respond using the deployed model and the instructions configured in the code.

To stop the application, press `Ctrl+C`.

## How it works

At a high level, the application:

1. Loads configuration from environment variables and .NET User Secrets.
2. Creates an `AIProjectClient` using `AzureCliCredential`.
3. Uses `AgentFactory` and `AgentOptions` to configure an agent named `Writer`.
4. Sends user input to the agent with `RunAsync`.
5. Prints the response in the console.

## Troubleshooting

- **Missing endpoint or deployment name:** confirm that both configuration keys are set correctly.
- **Authentication errors:** run `az login` again and verify that the signed-in account has access to the Azure resource.
- **Access denied:** check the permissions assigned to your account for the Foundry project and model.
- **Deployment not found:** confirm that the deployment name exactly matches the name in your Foundry project.
- **Invalid endpoint:** make sure you copied the project endpoint, not a portal page URL or a model catalog URL.

## Security

- Never commit credentials, access tokens, or private configuration to source control.
- Keep `.env` files out of Git.
- Use .NET User Secrets for local development and an appropriate secure configuration service for deployed applications.

## Technologies

- C#
- .NET
- Microsoft Foundry
- Azure Identity
- Microsoft Agent Framework
