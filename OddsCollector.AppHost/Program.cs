using OddsCollector.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var settings = builder.Configuration.GetSection(AppHostSettings.Section);

var oddsApiKey = builder.AddParameter(AppHostSettings.OddsApiKeyParameter, secret: true);

var cosmos = builder.AddAzureCosmosDB(AppHostSettings.CosmosResource)
    .RunAsEmulator(emulator => emulator
        .WithGatewayPort(AppHostSettings.CosmosGatewayPort)
        .WithEndpoint("emulator", endpoint => endpoint.IsProxied = false));

var database = cosmos.AddCosmosDatabase("database", AppHostSettings.DatabaseName);
database.AddContainer("event-predictions", "/id", AppHostSettings.EventPredictionsContainer);
database.AddContainer("event-results", "/id", AppHostSettings.EventResultsContainer);

var serviceBus = builder.AddAzureServiceBus(AppHostSettings.ServiceBusResource).RunAsEmulator();

serviceBus.AddServiceBusQueue("queue", AppHostSettings.Queue)
    .WithProperties(queue =>
    {
        queue.MaxDeliveryCount = 3;
        queue.LockDuration = TimeSpan.FromMinutes(1);
        queue.DefaultMessageTimeToLive = TimeSpan.FromHours(1);
    });

var functions = builder.AddAzureFunctionsProject<Projects.OddsCollector_Functions>(AppHostSettings.FunctionsResource)
    .WithReference(cosmos, "CosmosDb__Connection")
    .WithReference(serviceBus, "ServiceBus__Connection")
    .WithEnvironment("CosmosDb__Database", AppHostSettings.DatabaseName)
    .WithEnvironment("CosmosDb__EventPredictionsContainer", AppHostSettings.EventPredictionsContainer)
    .WithEnvironment("CosmosDb__EventResultsContainer", AppHostSettings.EventResultsContainer)
    .WithEnvironment("ServiceBus__Queue", AppHostSettings.Queue)
    .WithEnvironment("AzureFunctionsJobHost__extensions__cosmosDB__connectionMode", "Gateway")
    .WithEnvironment("OddsApiClient__ApiKey", oddsApiKey)
    .WithEnvironment("OddsApiClient__Leagues", settings["Leagues"] ?? "soccer_epl")
    .WithEnvironment("UpcomingEventsFunction__TimerInterval",
        settings["UpcomingEventsTimerInterval"] ?? "0 0 */6 * * *")
    .WithEnvironment("EventResultsFunction__TimerInterval", settings["EventResultsTimerInterval"] ?? "0 0 6 * * *")
    .WithEnvironment("APPLICATIONINSIGHTS_CONNECTION_STRING",
        settings["ApplicationInsightsConnectionString"] ?? AppHostSettings.UnreachableApplicationInsights)
    .WaitFor(cosmos)
    .WaitFor(serviceBus);

if (settings["OddsApiBaseUrl"] is { Length: > 0 } oddsApiBaseUrl)
{
    functions.WithEnvironment("OddsApiClient__BaseUrl", oddsApiBaseUrl);
}

builder.Build().Run();
