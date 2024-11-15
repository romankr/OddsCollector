**Odds Collector** is a software project aimed at gathering and analyzing odds data for various sports from [The Odds API](https://the-odds-api.com/) while leveraging the infrastructure of Microsoft Azure. This application provides an instrument to access historical odds information and predictions for sporting events.

# Purpose

The project is designed to have fun with algorythm described in "[Beating the bookies with their own numbers - and how the online sports betting market is rigged](https://www.researchgate.net/publication/320296375_Beating_the_bookies_with_their_own_numbers_-_and_how_the_online_sports_betting_market_is_rigged)" using The Odds API as the main source of odds data. There is also an improvement upon the original algorythm that increases prediction accuracy from 40% up to 60-70%.

# Features

**Historical Data**: Users can access historical odds data, allowing for in-depth analysis and trend identification.

**The Odds API Integration**: Potential to collect data from over 70 sports and over 40 bookmakers.

**Azure Functions Integration**: Optimizes resource consumption by executing specific functions in response to events, ensuring cost-effectiveness and streamlined execution.

**Azure Cosmos DB for Data Storage**: Utilizing Azure Cosmos DB as the backend database.

# Prerequisites

- [.NET 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Azure Development Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference?tabs=blob&pivots=programming-language-csharp#development-tools)
- (Optional) [NSwag Studio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio). The repository already has [The Odds API C# client](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/WebApiClient.cs). However, you can use [parameters.nswag](https://github.com/romankr/OddsCollector/blob/master/OddsCollector.Functions/OddsApi/WebApi/parameters.nswag) to modify it.
- (Optional) [Jupyter](https://jupyter.org/). Required for [analytics.ipynb](https://github.com/romankr/OddsCollector/blob/master/analytics.ipynb). 

# Local development

[Code and test Azure Functions locally](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-local)

# Build

```
dotnet build
```

# Deployment

[Deployment technologies in Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-deployment-technologies?tabs=windows)

# Special thanks

Special thanks to JetBrains and their [Open Source Support Program](https://www.jetbrains.com/community/opensource/#support) for providing a free licence for their products.

![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png)
