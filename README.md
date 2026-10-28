[![codecov](https://codecov.io/gh/romankr/OddsCollector/graph/badge.svg?branch=master)](https://codecov.io/gh/romankr/OddsCollector)

# About

Odds Collector is an Azure Functions app that collects football (soccer) odds from [The Odds API](https://the-odds-api.com/) and predicts match outcomes using the method from "[Beating the bookies with their own numbers - and how the online sports betting market is rigged](https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged)".

Unlike the paper, it does not look for value bets: it averages the bookmakers' implied probabilities, subtracts the paper's intercepts and predicts the single most likely outcome for every match, without using the best available odds. Only football leagues are supported.

# Prerequisites

- [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) (`func`)
- [Docker](https://www.docker.com/), for the integration tests and for running the app locally with the emulators
- (Optional) [NSwag Studio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio). The repository already has [The Odds API C# client](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/WebApiClient.cs). However, you can use [parameters.nswag](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/parameters.nswag) to modify it.

# Configuration

## Application settings

Locally the settings go in `OddsCollector.Functions/local.settings.json` under `Values`; in Azure they are the
function app's application settings. On a Linux plan, write `:` in a setting name as `__`
(for example `OddsApiClient__ApiKey`). All settings are required.

| Setting | Purpose |
| --- | --- |
| `OddsApiClient:ApiKey` | The Odds API key. In Azure keep it in Key Vault and use a [Key Vault reference](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references). |
| `OddsApiClient:Leagues` | Semicolon-separated `soccer_*` sport keys, for example `soccer_epl;soccer_spain_la_liga`. Any other sport key fails startup validation. |
| `CosmosDb:Connection` | Cosmos DB connection string. |
| `CosmosDb:Database` | Cosmos DB database name. |
| `CosmosDb:EventPredictionsContainer` | Container for predictions. |
| `CosmosDb:EventResultsContainer` | Container for completed results. |
| `ServiceBus:Connection` | Service Bus connection string. |
| `ServiceBus:Queue` | Queue for upcoming events. |
| `UpcomingEventsFunction:TimerInterval` | How often upcoming events are collected (NCRONTAB or `TimeSpan`). |
| `EventResultsFunction:TimerInterval` | How often results are collected (NCRONTAB or `TimeSpan`). Keep it under 3 days, or some results are never stored. |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights connection string. |
| `AzureWebJobsStorage` | Storage account for the Functions host. |
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated`. |

## Managed identity in Azure

Instead of connection strings, turn on the function app's managed identity, grant it the roles below and use
[identity-based connections](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference#configure-an-identity-based-connection):

| Instead of | Set | Role |
| --- | --- | --- |
| `CosmosDb:Connection` | `CosmosDb:Connection__accountEndpoint` | Cosmos DB Built-in Data Contributor |
| `ServiceBus:Connection` | `ServiceBus:Connection__fullyQualifiedNamespace` | Azure Service Bus Data Sender and Data Receiver |
| `AzureWebJobsStorage` | `AzureWebJobsStorage__accountName` | Storage Blob Data Owner |

## Running locally with the emulators

`OddsCollector.AppHost` is an [Aspire](https://aspire.dev/) AppHost that runs the function app in the Azure
Functions host against Microsoft's emulators: Cosmos DB (Linux, vNext), Service Bus and Azurite. It creates the
database, the containers and the queue, and passes every application setting except the Odds API key:

```
dotnet user-secrets --project OddsCollector.AppHost set "Parameters:odds-api-key" "<your key>"
dotnet run --project OddsCollector.AppHost
```

Optional settings, in `OddsCollector.AppHost` configuration (user secrets, `appsettings.json` or command line):
`OddsCollector:Leagues`, `OddsCollector:OddsApiBaseUrl`, `OddsCollector:UpcomingEventsTimerInterval`,
`OddsCollector:EventResultsTimerInterval` and `OddsCollector:ApplicationInsightsConnectionString`.

Port 8081 must be free: the Cosmos DB emulator is published on its own gateway port, because the Functions host
connects to the address the emulator advertises. The emulator serves HTTPS with the ASP.NET Core development
certificate, so create and trust it once with `dotnet dev-certs https --trust`. On Linux, first add the
certificate's trust directory to `SSL_CERT_DIR` (for example
`export SSL_CERT_DIR="$HOME/.aspnet/dev-certs/trust:/usr/lib/ssl/certs"`) and keep it set when running the app or
the tests; otherwise the certificate is only partially trusted and .NET does not accept it.

## Integration tests

The integration tests start `OddsCollector.AppHost` with
[Aspire.Hosting.Testing](https://learn.microsoft.com/dotnet/aspire/testing/overview), point it at a stub of The Odds
API and do not read `local.settings.json`. They need the same prerequisites as running locally: Docker, Azure
Functions Core Tools, a free port 8081 and the development certificate.

# Build and test

```
dotnet build
dotnet test --filter "TestCategory!=Integration"
dotnet test OddsCollector.Functions.IntegrationTests
```

The first integration test run downloads the emulator images, which takes a few minutes.

# Special thanks

Special thanks to JetBrains and their [Open Source Support Program](https://www.jetbrains.com/community/opensource/#support) for providing a free license for their products.

![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png)
