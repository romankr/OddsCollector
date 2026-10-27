namespace OddsCollector.AppHost;

/// <summary>
///     Names shared by the AppHost and the integration tests that start it.
/// </summary>
public static class AppHostSettings
{
    public const string Section = "OddsCollector";
    public const string OddsApiKeyParameter = "odds-api-key";
    public const string FunctionsResource = "functions";
    public const string CosmosResource = "cosmos";
    public const string ServiceBusResource = "servicebus";

    public const string DatabaseName = "OddsCollector";
    public const string EventPredictionsContainer = "EventPredictions";
    public const string EventResultsContainer = "EventResults";
    public const string Queue = "upcoming-events";

    public const int CosmosGatewayPort = 8081;

    public const string UnreachableApplicationInsights =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=http://127.0.0.1:9/";
}
