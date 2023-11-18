using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddsCollector.Functions.Notification.CommunicationServices;
using OddsCollector.Functions.Notification.CommunicationServices.Configuration;
using OddsCollector.Functions.Notification.CosmosDb;
using OddsCollector.Functions.Notification.CosmosDb.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // workaround for https://github.com/MicrosoftDocs/azure-docs/issues/32962
        services.Configure<CosmosDbOptions>(o =>
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            o.Container = Environment.GetEnvironmentVariable("CosmosDb:Container");
            o.Database = Environment.GetEnvironmentVariable("CosmosDb:Database");
            o.Connection = Environment.GetEnvironmentVariable("CosmosDb:Connection");
        });
        services.Configure<EmailSenderOptions>(o =>
        {
            o.Subject = Environment.GetEnvironmentVariable("EmailSender:Subject");
            o.Connection = Environment.GetEnvironmentVariable("EmailSender:Connection");
            o.RecipientAddress = Environment.GetEnvironmentVariable("EmailSender:RecipientAddress");
            o.SenderAddress = Environment.GetEnvironmentVariable("EmailSender:SenderAddress");
#pragma warning restore CS8601 // Possible null reference assignment.
        });
        services.AddSingleton<ICosmosDbClient, CosmosDbClient>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
