**Odds Collector** is a software project aimed at gathering and analyzing odds data for football (soccer) leagues from [The Odds API](https://the-odds-api.com/) while leveraging the infrastructure of Microsoft Azure. This application provides a tool for accessing historical odds information and predictions for football matches.

# Purpose

The project is designed to implement the algorithm described in "[Beating the bookies with their own numbers - and how the online sports betting market is rigged](https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged)" using The Odds API as the main source of odds data.

# Supported sports

Only football (soccer) leagues are supported, and this is by design. See `OddsApiClient:Leagues` in
[Configuration](#configuration).

# Features

**Historical Data**: Users can access historical odds data, allowing for in-depth analysis and trend identification.

**The Odds API Integration**: Collects head-to-head odds for football (soccer) leagues from over 40 bookmakers.

**Azure Functions Integration**: Optimizes resource consumption by executing specific functions in response to events, ensuring cost-effectiveness and streamlined execution.

**Azure Cosmos DB for Data Storage**: Utilizing Azure Cosmos DB as the backend database.

# Prerequisites

- [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Azure Development Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference?tabs=blob&pivots=programming-language-csharp#development-tools)
- (Optional) [NSwag Studio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio). The repository already has [The Odds API C# client](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/WebApiClient.cs). However, you can use [parameters.nswag](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/parameters.nswag) to modify it.

# Configuration

## Where settings live

Locally the settings belong in `OddsCollector.Functions/local.settings.json` under `Values`; in Azure they are the
function app's application settings. On a Linux plan, write the `:` in a setting name as `__`
(for example `OddsApiClient__ApiKey`); both forms are read the same way. The integration tests do not read
`local.settings.json`; see [Integration test settings](#integration-test-settings).

## Application settings

Every setting below has to be present before the app runs, except where it says optional. A missing
`OddsApiClient:ApiKey` or `OddsApiClient:Leagues` stops the app at startup.

| Setting | Purpose |
| --- | --- |
| `OddsApiClient:ApiKey` | The Odds API key. In Azure keep it in Key Vault and set the app setting to a [Key Vault reference](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references). |
| `OddsApiClient:BaseUrl` | Optional. Base URL of The Odds API, `https://api.the-odds-api.com` when not set. Integration tests point it at a stub. |
| `OddsApiClient:Leagues` | Semicolon-separated football (soccer) sport keys to collect, for example `soccer_epl;soccer_spain_la_liga`. Put only `soccer_*` keys here; other sports are not supported, see [Supported sports](#supported-sports). Empty entries are dropped and surrounding whitespace is trimmed. |
| `CosmosDb:Connection` | Connection string for the Cosmos DB account, or an [identity-based connection](#identity-based-connections). |
| `CosmosDb:Database` | Cosmos DB database name. |
| `CosmosDb:EventPredictionsContainer` | Container `PredictionFunction` writes predictions to and `PredictionsHttpFunction` reads them from. |
| `CosmosDb:EventResultsContainer` | Container `EventResultsFunction` writes completed results to. |
| `ServiceBus:Connection` | Connection string for the Service Bus namespace, or an [identity-based connection](#identity-based-connections). |
| `ServiceBus:Queue` | Queue carrying upcoming events from `UpcomingEventsFunction` to `PredictionFunction`. |
| `EventResultsFunction:TimerInterval` | How often results are collected, as an NCRONTAB expression or a `TimeSpan`. Each run asks The Odds API for games completed in the last 3 days, so keep the interval shorter than that, or results of games completed in between are never stored. |
| `UpcomingEventsFunction:TimerInterval` | How often upcoming events are collected, as an NCRONTAB expression or a `TimeSpan`. |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights connection string, read by the OpenTelemetry exporter. |
| `AzureWebJobsStorage` | Storage account the Functions host uses for timer schedules and leases, or an [identity-based connection](#identity-based-connections). Give the function app its own storage account in the same region. |
| `FUNCTIONS_WORKER_RUNTIME` | Has to be `dotnet-isolated`. |

Do not set `AzureWebJobsDashboard`; it is obsolete.

## Identity-based connections

In Azure, prefer a managed identity to connection strings. Turn on the function app's managed identity, grant it the
roles below and replace each connection string with the setting next to it; the bindings read
[identity-based connections](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference#configure-an-identity-based-connection)
from the same setting prefix.

| Instead of | Set | Role to grant |
| --- | --- | --- |
| `CosmosDb:Connection` | `CosmosDb:Connection__accountEndpoint` | Cosmos DB Built-in Data Contributor |
| `ServiceBus:Connection` | `ServiceBus:Connection__fullyQualifiedNamespace` | Azure Service Bus Data Sender and Azure Service Bus Data Receiver |
| `AzureWebJobsStorage` | `AzureWebJobsStorage__accountName` | Storage Blob Data Owner |

## host.json

| Setting | Value |
| --- | --- |
| `functionTimeout` | `00:10:00`: a run that takes longer is stopped. |
| `telemetryMode` | `OpenTelemetry`: the host sends its telemetry through OpenTelemetry, like the worker. |

## Integration test settings

`OddsCollector.Functions.IntegrationTests` configures the function app itself. Optional environment variables:

| Variable | Purpose |
| --- | --- |
| `ODDSCOLLECTOR_FUNC_PATH` | Path to `func`. |
| `ODDSCOLLECTOR_FUNCTION_APP_DIRECTORY` | Directory of the built function app. |
| `ODDSCOLLECTOR_COSMOSDB_EMULATOR_IMAGE` | Cosmos DB emulator image. |

# Local development

[Code and test Azure Functions locally](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-local)

# Build

```
dotnet build
```

# Tests

Unit tests:

```
dotnet test --filter "TestCategory!=Integration"
```

`OddsCollector.Functions.IntegrationTests` runs every function end to end in a local Azure
environment it starts itself: Azurite, the Service Bus emulator and the Cosmos DB emulator in Docker,
a stub of The Odds API and the Azure Functions host. It needs
[Docker](https://www.docker.com/) and
[Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
(`func`) in `PATH`; the first run downloads the emulator images, which takes a few minutes.

```
dotnet test OddsCollector.Functions.IntegrationTests
```

See [Integration test settings](#integration-test-settings) for the environment variables they read.

# Deployment

[Deployment technologies in Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-deployment-technologies?tabs=windows)

The code follows [Best practices for reliable Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-best-practices?tabs=csharp)
and the [Azure Well-Architected Framework guide for Azure Functions](https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-functions).
Part of that guidance is about the Azure resources rather than the code; set up the function app as follows, and its
settings as described in [Configuration](#configuration).

- **Hosting plan.** The workload is a couple of timers, a queue and a rarely called HTTP endpoint, so
  the [Flex Consumption plan](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan)
  fits it best. Turn on zone redundancy where the region supports it.
- **Secrets.** Use a managed identity and Key Vault rather than secrets in app settings; see
  [Identity-based connections](#identity-based-connections) and `OddsApiClient:ApiKey` in
  [Application settings](#application-settings).
- **Network and access.** Require HTTPS with TLS 1.2 or later and turn off FTP and basic authentication for
  deployment. `PredictionsHttpFunction` needs a function key; put it behind Microsoft Entra ID authentication or
  API Management if it is exposed beyond trusted callers.
- **Reliability.** `UpcomingEventsFunction` and `EventResultsFunction` have a built-in retry policy for failed runs;
  The Odds API calls are retried by the standard HTTP resilience handler; Service Bus redelivers a failed
  `PredictionFunction` message and dead-letters it after the queue's max delivery count. Cosmos DB documents are keyed
  by the event id, so a retried run or a duplicate message overwrites a document rather than adding another.
- **Deployment.** Deploy the package from CI/CD (run from package), and use rolling updates on Flex Consumption
  or a staging slot on other plans for zero-downtime releases.
- **Monitoring.** Add alerts for failed invocations, dead-lettered messages and a low quota: the
  `Odds API credits` log carries the counts as `RemainingCredits`, `UsedCredits` and `LastCallCredits`
  (`customDimensions` in Application Insights).

# Special thanks

Special thanks to JetBrains and their [Open Source Support Program](https://www.jetbrains.com/community/opensource/#support) for providing a free license for their products.

![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png)
