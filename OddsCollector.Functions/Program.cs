using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddsCollector.Functions.CommunicationServices;
using OddsCollector.Functions.CommunicationServices.Configuration;
using OddsCollector.Functions.CosmosDb;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.Strategies;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.Configure<EmailSenderOptions>(o =>
        {
            // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
            o.SetSubject(Environment.GetEnvironmentVariable("EmailSender:Subject"));
            o.SetRecipientAddress(Environment.GetEnvironmentVariable("EmailSender:RecipientAddress"));
            o.SetSenderAddress(Environment.GetEnvironmentVariable("EmailSender:SenderAddress"));
        });
        services.AddSingleton(
            // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
            new EmailClient(Environment.GetEnvironmentVariable("EmailSender:Connection"))
        );
        services.AddSingleton(
            ContainerFactory.CreateContainer(
                // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
                Environment.GetEnvironmentVariable("CosmosDb:Connection"),
                Environment.GetEnvironmentVariable("CosmosDb:Database"),
                Environment.GetEnvironmentVariable("CosmosDb:EventPredictionsContainer")
            ));
        services.AddSingleton<ICosmosDbClient, CosmosDbClient>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddSingleton<IPredictionStrategy, AdjustedConsensusStrategy>();
        services.AddOddsApiClientWithDependencies();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
