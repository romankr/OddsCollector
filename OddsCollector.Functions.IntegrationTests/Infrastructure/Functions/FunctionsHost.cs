using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using OddsCollector.AppHost;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Polling;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.Functions;

// The function app that the AppHost runs in the Azure Functions host: its HTTP endpoint and its console log.
internal sealed class FunctionsHost : IAsyncDisposable
{
    private const int MaxLogLines = 2000;

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

    private readonly ConcurrentQueue<(long Number, string Line)> _logs = new();
    private readonly CancellationTokenSource _stopWatchingLogs = new();
    private readonly Task _watchingLogs;
    private long _loggedLines;

    private FunctionsHost(DistributedApplication app)
    {
        Client = app.CreateHttpClient(AppHostSettings.FunctionsResource);
        Client.Timeout = TimeSpan.FromSeconds(30);

        _watchingLogs = WatchLogsAsync(app.Services.GetRequiredService<ResourceLoggerService>(),
            _stopWatchingLogs.Token);
    }

    public HttpClient Client { get; }

    public string Logs => string.Join(Environment.NewLine, _logs.Select(l => l.Line));

    // Lines are numbered from the host start, so a test can read what was logged after a given point even though
    // only the last MaxLogLines lines are kept.
    public long LoggedLines => Interlocked.Read(ref _loggedLines);

    public async ValueTask DisposeAsync()
    {
        await _stopWatchingLogs.CancelAsync();
        await _watchingLogs;

        _stopWatchingLogs.Dispose();
        Client.Dispose();
    }

    public static async Task<FunctionsHost> StartAsync(DistributedApplication app, CancellationToken cancellationToken)
    {
        await app.ResourceNotifications.WaitForResourceAsync(AppHostSettings.FunctionsResource,
            KnownResourceStates.Running, cancellationToken);

        var host = new FunctionsHost(app);

        try
        {
            await host.WaitUntilRunningAsync(cancellationToken);
        }
        catch
        {
            await host.DisposeAsync();
            throw;
        }

        return host;
    }

    public async Task InvokeAsync(string functionName, CancellationToken cancellationToken)
    {
        using var response = await Client.PostAsJsonAsync($"admin/functions/{functionName}", new { input = "" },
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Invoking {functionName} returned {(int)response.StatusCode}: {body}{Environment.NewLine}{Logs}");
        }
    }

    public IReadOnlyList<string> GetLogLinesAfter(long loggedLines)
    {
        return [.. _logs.Where(l => l.Number > loggedLines).Select(l => l.Line)];
    }

    public async Task<EventPrediction[]?> TryGetPredictionsAsync(CancellationToken cancellationToken)
    {
        using var response = await Client.GetAsync($"api/{nameof(PredictionsHttpFunction)}", cancellationToken);

        return response.StatusCode == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<EventPrediction[]>(cancellationToken)
            : null;
    }

    // Aspire reports the resource as running once func has started, before the host has loaded the functions.
    private async Task WaitUntilRunningAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Eventually.GetAsync(async token =>
            {
                using var response = await Client.GetAsync("admin/host/status", token);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                using var status = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));

                return status.RootElement.TryGetProperty("state", out var state) && state.GetString() == "Running"
                    ? state.GetString()
                    : null;
            }, StartupTimeout, "the Azure Functions host to start", cancellationToken);
        }
        catch (TimeoutException exception)
        {
            throw new InvalidOperationException(
                $"The Azure Functions host did not start.{Environment.NewLine}{Logs}", exception);
        }
    }

    private async Task WatchLogsAsync(ResourceLoggerService loggers, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var lines in loggers.WatchAsync(AppHostSettings.FunctionsResource)
                               .WithCancellation(cancellationToken))
            {
                foreach (var line in lines)
                {
                    Log(line.Content);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void Log(string line)
    {
        _logs.Enqueue((Interlocked.Increment(ref _loggedLines), line));

        while (_logs.Count > MaxLogLines)
        {
            _logs.TryDequeue(out _);
        }

        TestContext.Progress.WriteLine($"[func] {line}");
    }
}
