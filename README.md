**Odds Collector** is a software project aimed at gathering and analyzing odds data for various sports from [The Odds API](https://the-odds-api.com/) while leveraging the infrastructure of Microsoft Azure. This application provides a tool for accessing historical odds information and predictions for sporting events.

# Purpose

The project is designed to have fun with the algorithm described in "[Beating the bookies with their own numbers - and how the online sports betting market is rigged](https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged)" using The Odds API as the main source of odds data. There is also an improvement on the original algorithm that increases prediction accuracy from 40% to 60-70%.

# Features

**Historical Data**: Users can access historical odds data, allowing for in-depth analysis and trend identification.

**The Odds API Integration**: Potential to collect data from over 70 sports and over 40 bookmakers.

**Azure Functions Integration**: Optimizes resource consumption by executing specific functions in response to events, ensuring cost-effectiveness and streamlined execution.

**Azure Cosmos DB for Data Storage**: Utilizing Azure Cosmos DB as the backend database.

# Prerequisites

- [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Azure Development Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference?tabs=blob&pivots=programming-language-csharp#development-tools)
- (Optional) [NSwag Studio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio). The repository already has [The Odds API C# client](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/WebApiClient.cs). However, you can use [parameters.nswag](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/parameters.nswag) to modify it.

# Application settings

Every setting below has to be present before the app runs. Locally they belong in
`OddsCollector.Functions/local.settings.json` under `Values`; in Azure they are the function
app's application settings.

| Setting | Purpose |
| --- | --- |
| `OddsApiClient:ApiKey` | The Odds API key. |
| `OddsApiClient:Leagues` | Semicolon-separated sport keys to collect, for example `soccer_epl;soccer_spain_la_liga`. Empty entries are dropped and surrounding whitespace is trimmed. |
| `CosmosDb:Connection` | Connection string for the Cosmos DB account. |
| `CosmosDb:Database` | Cosmos DB database name. |
| `CosmosDb:EventPredictionsContainer` | Container `PredictionFunction` writes predictions to and `PredictionsHttpFunction` reads them from. |
| `CosmosDb:EventResultsContainer` | Container `EventResultsFunction` writes completed results to. |
| `ServiceBus:Connection` | Connection string for the Service Bus namespace. |
| `ServiceBus:Queue` | Queue carrying upcoming events from `UpcomingEventsFunction` to `PredictionFunction`. |
| `EventResultsFunction:TimerInterval` | How often results are collected, as an NCRONTAB expression or a `TimeSpan`. |
| `UpcomingEventsFunction:TimerInterval` | How often upcoming events are collected, as an NCRONTAB expression or a `TimeSpan`. |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights connection string, read by the OpenTelemetry exporter. |
| `AzureWebJobsStorage` | Storage account the Functions host uses for timer schedules and leases. |
| `FUNCTIONS_WORKER_RUNTIME` | Has to be `dotnet-isolated`. |

# Local development

[Code and test Azure Functions locally](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-local)

# Build

```
dotnet build
```

# Deployment

[Deployment technologies in Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-deployment-technologies?tabs=windows)

# Special thanks

Special thanks to JetBrains and their [Open Source Support Program](https://www.jetbrains.com/community/opensource/#support) for providing a free license for their products.

![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png)
