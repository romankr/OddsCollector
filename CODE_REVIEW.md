# OddsCollector — Code Review (fourth pass)

**Reviewed:** 14 Aug 2026 · `cd1c1fb Fix code review issues` vs `b03df89`
**Scope:** the 17 changed files in that commit.
**Excluded:** `OddsApi/WebApi/WebApiClient.cs` (NSwag-generated).

> Still no .NET SDK in the review environment — nothing was compiled or run.

## Summary

Mostly deletion, and mostly the right deletions. Both pass-through processors are gone rather than patched, which resolves the duplication, the inconsistency and the tautological tests in one move. The dead master workflow is gone too.

One thing went with them that shouldn't have: the timer functions now have no success-path logging at all. That's the third round in which logging has been removed, and this time it was the last copy.

---

## Resolved

**The processor layer is gone.** `EventResultProcessor`, `UpcomingEventsProcessor` and both interfaces deleted; `EventResultsFunction` and `UpcomingEventsFunction` now inject `IEventResultsClient` / `IUpcomingEventsClient` directly. This was the better of the two options I offered — the indirection had no behaviour left to justify it, and the tests that went with it were asserting that NSubstitute works. Net −281 lines.

**`master-build.yml` deleted.** Clean answer to "master gets weaker checks than PRs" — rather than strengthening a workflow that duplicated the PR gate, it's removed. Worth confirming branch protection requires PRs into master; if direct pushes are allowed, nothing runs on them now.

**Coverage scoped to the production assembly.** `/p:Include="[OddsCollector.Functions]*"` with the threshold enforced in the same step, so the suite runs once.

**`OutcomeConverter` is now `sealed`,** consistent with the rest of the project.

---

## New

### 1. The timer functions have no success-path logging left (Medium)

`UpcomingEventsProcessor` was the last place the `No events received` / `{Length} event(s) received` pair lived. With it deleted, `UpcomingEventsFunction` and `EventResultsFunction` log exactly one thing between them:

```csharp
catch (Exception exception)
{
    logger.LogError(exception, "Failed to get events");
}
```

Nothing is recorded on the happy path. For a data-collection pipeline, "The Odds API returned an empty list" is the most likely silent failure — expired API key, exhausted quota, a league key that no longer resolves — and all of those now produce a successful invocation with zero events and no log line. You'd only notice from the absence of Cosmos documents.

Deleting the processors was right; the logging just needed somewhere else to live. The clients already have the data:

```csharp
public async Task<EventResult[]> GetEventResultsAsync(CancellationToken cancellationToken)
{
    ...
    if (result.Count == 0)
    {
        logger.LogWarning("No events received");
    }
    else
    {
        logger.LogInformation("{Length} event(s) received", result.Count);
    }

    return [.. result];
}
```

Either client or function is a fine home — the point is that one of them should say how much data came back.

### 2. `AddFunctionProcessors` now registers one service (Low)

```csharp
public static void AddFunctionProcessors(this IServiceCollection services)
{
    services.AddSingleton<IPredictionProcessor, PredictionProcessor>();
}
```

A plural name, its own file and its own namespace for a single registration. Folding it into `AddPredictionStrategy` — which already registers the rest of the prediction pipeline — would put the whole feature in one place.

---

## Carried forward

**`OperationCanceledException` is logged as an error, and it's now the only thing these functions log.** `ThrowIfCancellationRequested()` in the clients propagates into the generic `catch (Exception)` in both timer functions, producing an `Error` with a stack trace on every routine host shutdown. Combined with finding 1, the entire log output of these two functions is now error-shaped, with no successful-run signal to contrast against. Still a one-line fix:

```csharp
catch (OperationCanceledException) { throw; }
catch (Exception exception) { logger.LogError(exception, "Failed to get events"); }
```

**Dead-lettering still fires on every exception type.** `PredictionProcessor.cs:29-41` — the nested try/catch means a failed settlement can no longer abort the batch, which was the dangerous part. But `CompleteMessageAsync` is still inside the try and all exceptions still route to dead-lettering, so a transient completion failure with a valid lock permanently discards a good message. An exception filter separating `JsonException` / `ArgumentException` from `ServiceBusException` closes it.

**`Outcome` still holds two vocabularies.** `OriginalCompletedEventConverter.cs:23` — `SetOutcome(converter.GetOutcome(originalEvent.Scores))` writes a team name, while `ScoreCalculator.cs:11-13` gives predictions `OutcomeTypes.Draw` / `AwayTeam` / `HomeTeam`. Same property name on both documents, not comparable, which blocks the accuracy measurement the project exists for. `Anonymous3.Home_team` / `Away_team` are in scope at the call site; the mapping is about six lines.

**The draw lookup still throws.** `OddConverter.cs:22` — `outcomes.First(o => o.Name == oddType)` called unconditionally for `Draw`. The exception escapes the lazy enumerable and discards every event for every league in the run.

**No DI resolution test.** `Tests/OddsApi/Configuration/ServiceCollectionExtensions.cs` has no `BuildServiceProvider` or `GetRequiredService` anywhere — still purely descriptor assertions.

**Smaller, all unchanged:** `OutcomeConverter` is named after a type it never touches (`OddConverter` is the one consuming `Outcome`); `OddsApiClientOptions.ApiKey`'s public setter bypasses `SetApiKey`; `Leagues` is a case-sensitive `HashSet`; the `0.057` / `0.034` / `0.037` adjustments are undocumented; the `1/mean(odds)` vs `mean(1/odds)` question in `ScoreCalculator` is unanswered; and the PR workflow file still has no trailing newline.

---

## Suggested order

1. Put the event-count logging back, in the clients.
2. Let `OperationCanceledException` through the two timer functions.
3. The `Outcome` mapping — the largest remaining functional gap, and cheapest to do while the stored data is small.
4. The dead-letter exception filter.
5. The draw `.First()` and a DI resolution test — both small, both close out first-pass findings.

Also worth watching on the next CI run: with the test assembly no longer counted, the coverage figure will be lower than before, and 70% may not hold on the first attempt.
