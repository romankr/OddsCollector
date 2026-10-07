using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Network;
using OddsCollector.Functions.IntegrationTests.Infrastructure.Polling;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.Functions;

/// <summary>
///     Runs the built function app in the Azure Functions Core Tools host (<c>func</c>), the same
///     host <c>func start</c> uses for local development.
/// </summary>
internal sealed class FunctionsHost : IAsyncDisposable
{
    private const string FuncPathVariable = "ODDSCOLLECTOR_FUNC_PATH";
    private const string AppDirectoryVariable = "ODDSCOLLECTOR_FUNCTION_APP_DIRECTORY";
    private const int MaxLogLines = 2000;

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

    private readonly string _appDirectory;
    private readonly ConcurrentQueue<string> _logs = new();
    private readonly Process _process;

    private FunctionsHost(Process process, string appDirectory, Uri baseAddress)
    {
        _process = process;
        _appDirectory = appDirectory;
        BaseAddress = baseAddress;
        Client = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(30) };
    }

    public Uri BaseAddress { get; }

    public HttpClient Client { get; }

    public string Logs => string.Join(Environment.NewLine, _logs);

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        catch (InvalidOperationException)
        {
            // The process was never started or has already exited.
        }

        _process.Dispose();

        TryDeleteDirectory(_appDirectory);
    }

    public static async Task<FunctionsHost> StartAsync(IReadOnlyDictionary<string, string> settings,
        CancellationToken cancellationToken)
    {
        var appDirectory = CopyFunctionApp();
        var port = FreePort.Get();

        var startInfo = CreateStartInfo(appDirectory, port);

        foreach (var (name, value) in settings)
        {
            startInfo.Environment[name] = value;
        }

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        var host = new FunctionsHost(process, appDirectory, new Uri($"http://localhost:{port}/"));

        process.OutputDataReceived += (_, e) => host.Log(e.Data);
        process.ErrorDataReceived += (_, e) => host.Log(e.Data);

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException($"Failed to start {startInfo.FileName}");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await host.WaitUntilRunningAsync(cancellationToken);
        }
        catch
        {
            await host.DisposeAsync();
            throw;
        }

        return host;
    }

    /// <summary>
    ///     Runs a function that is not HTTP triggered, such as a timer function, right away.
    /// </summary>
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

    /// <summary>
    ///     Calls <see cref="PredictionsHttpFunction" />.
    /// </summary>
    /// <returns>The predictions, or <c>null</c> when the function does not answer with 200 OK.</returns>
    public async Task<EventPrediction[]?> TryGetPredictionsAsync(CancellationToken cancellationToken)
    {
        using var response = await Client.GetAsync($"api/{nameof(PredictionsHttpFunction)}", cancellationToken);

        return response.StatusCode == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<EventPrediction[]>(cancellationToken)
            : null;
    }

    private async Task WaitUntilRunningAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Eventually.GetAsync(async token =>
            {
                if (_process.HasExited)
                {
                    throw new InvalidOperationException($"func exited with code {_process.ExitCode}");
                }

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
        catch (Exception exception) when (exception is TimeoutException or InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"The Azure Functions host did not start.{Environment.NewLine}{Logs}", exception);
        }
    }

    private void Log(string? line)
    {
        if (line is null)
        {
            return;
        }

        _logs.Enqueue(line);

        while (_logs.Count > MaxLogLines)
        {
            _logs.TryDequeue(out _);
        }

        TestContext.Progress.WriteLine($"[func] {line}");
    }

    private static ProcessStartInfo CreateStartInfo(string appDirectory, int port)
    {
        var func = FindFunc();
        var arguments = $"host start --port {port}";

        // A .cmd shim (npm installs func that way on Windows) only runs through the command interpreter.
        var startInfo = func.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase)
            ? new ProcessStartInfo("cmd.exe", $"/d /c \"\"{func}\" {arguments}\"")
            : new ProcessStartInfo(func, arguments);

        startInfo.WorkingDirectory = appDirectory;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;

        return startInfo;
    }

    private static string FindFunc()
    {
        var configured = Environment.GetEnvironmentVariable(FuncPathVariable);

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        string[] names = OperatingSystem.IsWindows() ? ["func.exe", "func.cmd"] : ["func"];

        var paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return paths.SelectMany(path => names.Select(name => Path.Combine(path, name))).FirstOrDefault(File.Exists)
               ?? throw new InvalidOperationException(
                   "Azure Functions Core Tools (func) were not found in PATH. Install them " +
                   "(https://learn.microsoft.com/azure/azure-functions/functions-run-local) " +
                   $"or set {FuncPathVariable} to the func executable.");
    }

    /// <summary>
    ///     Copies the build output of the function app to a temporary directory without
    ///     local.settings.json, so a developer's own settings never reach the tests and the tests never
    ///     write to the developer's resources.
    /// </summary>
    private static string CopyFunctionApp()
    {
        var source = Environment.GetEnvironmentVariable(AppDirectoryVariable);

        if (string.IsNullOrWhiteSpace(source))
        {
            source = typeof(FunctionsHost).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(a => a.Key == "FunctionAppDirectory").Value!;
        }

        source = Path.GetFullPath(source);

        if (!File.Exists(Path.Combine(source, "functions.metadata")))
        {
            throw new InvalidOperationException(
                $"No built function app in {source}. Build the solution or set {AppDirectoryVariable}.");
        }

        var target = Path.Combine(Path.GetTempPath(), $"oddscollector-functions-{Guid.NewGuid():N}");

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);

            if (relative.Equals("local.settings.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var destination = Path.Combine(target, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination);
        }

        return target;
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
            // Files can stay locked for a moment after the host exits; the temp directory is cleaned up later.
        }
        catch (UnauthorizedAccessException)
        {
            // Same as above.
        }
    }
}
