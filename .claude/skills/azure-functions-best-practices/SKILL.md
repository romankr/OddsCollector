---
name: azure-functions-best-practices
description: Code-level rules for reliable Azure Functions (isolated worker, C#) in OddsCollector. Use whenever you add or change a function, trigger, binding, host.json, DI registration or outbound client in OddsCollector.Functions.
---

# Azure Functions best practices (code level)

Derived from Microsoft Learn, "Best practices for reliable Azure Functions"
(https://learn.microsoft.com/en-us/azure/azure-functions/functions-best-practices?tabs=csharp),
applied to this repo: .NET 10 isolated worker, timer + Service Bus triggers, Service Bus and Cosmos DB
output bindings, HTTP calls to The Odds API. For architecture/platform decisions see the
`azure-functions-well-architected` skill.

Assume instances restart, move and scale out at any moment, and that dependencies fail transiently.

## Write robust functions

- **Stateless.** No in-memory state that must survive between invocations; static/singleton state is only
  for caches and clients. Anything durable goes to Cosmos DB or Service Bus.
- **Short-running.** Keep each invocation well within `functionTimeout` in `host.json` (currently 10 min).
  Split long work into steps connected by a queue (as `UpcomingEventsFunction` → queue → `PredictionFunction`)
  rather than one big run. Large payloads: fan out into messages, don't hold everything in memory.
- **Defensive.** Expect partial failure: one league failing must not lose the others; validate input early.
- **Idempotent.** Triggers deliver at least once and retries re-run the whole function. Re-processing the same
  message/timer tick must not duplicate or corrupt data — use stable document ids (upserts keyed by event id),
  and skip work that is already done or no longer valid.
- **Poison messages.** A message that fails the same way on every delivery (bad JSON, invalid model) is
  dead-lettered with a reason instead of being retried until max delivery count.

## Retries and transient faults

- Outbound HTTP goes through `IHttpClientFactory` with `AddStandardResilienceHandler()`; don't hand-roll retries
  on top of it.
- Function-level retry (`[ExponentialBackoffRetry]` / `[FixedDelayRetry]`) only where re-running the whole
  function is safe and affordable; keep counts low where each retry costs Odds API quota. Document why in a comment.
- Service Bus already redelivers on failure; don't add function retries that multiply deliveries without reason.

## Concurrency, connections, async

- Async all the way; never block (`.Result`, `.Wait()`, `Thread.Sleep`).
- Accept the `CancellationToken` in `Run` and pass it to every async call; treat cancellation as a normal
  shutdown path (log at Information, don't fail noisily).
- Reuse clients: register `HttpClient`, Service Bus and Cosmos clients as typed/singleton via DI.
  Never `new HttpClient()` per invocation — it exhausts sockets/connection limits.
- Trigger concurrency and batching (Service Bus `maxConcurrentCalls`, prefetch, etc.) belong in `host.json`
  and apply to the whole app; change them deliberately and note the reason.

## Configuration

- All settings through app settings / `%Setting%` binding expressions, validated at startup
  (`ValidateOnStart`, `IValidateOptions<T>`). A missing required setting should stop the host, not fail later.
- Secrets via Key Vault references or identity-based connections — never in code, `host.json`
  or committed `local.settings.json`.
- Each function app gets its own storage account (`AzureWebJobsStorage`) in the same region.

## Organization and deployment

- The function app is the unit of deployment and scaling: functions in one app scale and deploy together.
  Keep related functions together; split into another app only for different scaling/security needs.
- Deploy as a package (run-from-package / zip) through CI; use slots or rolling updates for zero downtime.

## Observability

- Use `ILogger<T>` with structured message templates; include identifiers (event id, message id, league).
- Telemetry goes through OpenTelemetry → Azure Monitor (`telemetryMode: OpenTelemetry`,
  `UseAzureMonitorExporter()`); don't add a second telemetry pipeline.
- Log once at the place an error is handled; warnings for degraded-but-continuing runs
  (e.g. "No events received").
- If expected telemetry is missing, check sampling before adding more logs.

## Testing

- Unit-test functions by constructing them with substitutes (see `dotnet-unit-testing-best-practices`).
- Behavior that depends on the host, bindings, retries or dead-lettering is covered by
  `OddsCollector.Functions.IntegrationTests` running against the real Functions host.

## Checklist for a new or changed function

- [ ] Idempotent under redelivery and retry
- [ ] Bounded run time, no in-memory state across invocations
- [ ] `CancellationToken` accepted and passed through; no blocking calls
- [ ] Clients from DI, not created per call
- [ ] Retry strategy chosen and justified; poison inputs dead-lettered
- [ ] Settings referenced via `%...%`, validated at startup; no secrets in code
- [ ] Structured logs with ids; failures logged once
- [ ] Unit tests + integration test where host behavior matters
