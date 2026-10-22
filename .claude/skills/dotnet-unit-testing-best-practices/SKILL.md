---
name: dotnet-unit-testing-best-practices
description: Rules for writing and reviewing .NET unit and integration tests in OddsCollector (NUnit, NSubstitute, FluentAssertions). Use whenever you add, change, or review any test in OddsCollector.Functions.UnitTests or OddsCollector.Functions.IntegrationTests.
---

# .NET unit testing best practices

Based on Microsoft Learn, "Best practices for writing unit tests"
(https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices).
The article's samples use xUnit; the rules below are restated for this repo's stack:
**NUnit 4** (`[Test]`, `[TestCase]`), **NSubstitute** (`Substitute.For<T>()`),
**FluentAssertions** (`.Should()`), `FakeLogger<T>` from `Microsoft.Extensions.Diagnostics.Testing`.

## What a good unit test is

- **Fast** — milliseconds; the suite should be cheap to run after every change.
- **Isolated** — no file system, database, network, Service Bus, Cosmos DB or real clock.
- **Repeatable** — same result every run, on any machine, any day.
- **Self-checking** — passes or fails on its own, no human reading output.
- **Timely** — if a test is disproportionately hard to write, the production code probably needs a better seam.

Coverage is a hint, not a goal: high coverage does not prove the tests are good, and chasing the last few percent rarely pays off.

## Fakes, stubs, mocks — name by role

- **Stub** — replaces a dependency and feeds data in; never asserted on.
- **Mock** — a fake that the test asserts against (received calls, logged entries).
- **Fake** — umbrella term for either.

Name variables by how they are used in *this* test: `strategyStub`, `messageActionsMock`, `loggerMock`.
Calling a stub a mock (or vice versa) misleads the reader.

## Rules

### 1. Keep infrastructure out of unit tests
Unit tests must not touch real infrastructure. Depend on interfaces injected via constructor
(Explicit Dependencies Principle) and substitute them. Anything needing Docker, Azurite, Cosmos DB emulator,
Service Bus or Functions Core Tools belongs in `OddsCollector.Functions.IntegrationTests`
(`TestCategory=Integration`). The unit test project must not reference infrastructure-only packages.

### 2. Name tests `Method_Scenario_ExpectedBehavior`
The name is the documentation: a failing test should say which scenario broke.
For `[TestCase]` rows, give each case its own `TestName` in the same pattern.

```csharp
[Test]
public void GetScores_WithOneUnusableQuote_LeavesItOutOfTheConsensus()
```

### 3. Arrange / Act / Assert
Three visibly separated sections marked `// Arrange`, `// Act`, `// Assert`.
No assertions in Arrange or Act; no production calls in Assert. A short reason may follow
`// Assert:` when the expected value is not obvious.

### 4. Minimal inputs
Use the simplest data that exercises the behavior. Extra properties and arbitrary values hide intent
and make tests break for unrelated changes. Prefer shared builders (e.g. `ValidModels`) and override
only the fields the test is about with `with { ... }`.

### 5. No magic values
A literal whose meaning isn't obvious becomes a named constant (`private const string EventId = "eventId";`,
`private const double Precision = 0.0001;`) or is explained by a comment showing how it was derived.

### 6. No logic in tests
No `if`, `for`/`foreach`, `while`, `switch`, or computing the expected value with code in the test body.
Logic in a test can itself be wrong. Use `[TestCase]` / `[TestCaseSource]` to cover several inputs,
and write expected values as literals.

```csharp
[TestCase(0, TestName = "GetScores_WithZeroOdds_ScoresNothing")]
[TestCase(0.5, TestName = "GetScores_WithOddsBelowOne_ScoresNothing")]
public void GetScores_WithUnusableOdds_ScoresNothing(double value) { ... }
```

### 7. Helper methods over `[SetUp]` / `[TearDown]`
Build the system under test with private factory helpers called from each test
(e.g. `CreateFunction(strategyStub, loggerMock)`), not shared `[SetUp]` state.
Shared setup forces every test into the same arrangement, hides what a test really needs,
and risks state leaking between tests (fixtures run in parallel here: `ParallelScope.Fixtures`).

### 8. One Act per test
Exactly one call to the code under test. Several behaviors → several tests or `[TestCase]` rows.
Several *assertions* on the result of that one Act are fine — wrap them in `using var scope = new AssertionScope();`
so all failures are reported.

### 9. Test private behavior through the public API
Do not test private/internal helpers directly (no reflection, no `InternalsVisibleTo` just for that).
Exercise the public method that uses them and assert on its observable result.

### 10. Seams for statics and the environment
Never let a test depend on `DateTime.Now`/`UtcNow`, `Guid.NewGuid()`, environment variables, or other statics.
Inject `TimeProvider` (or an interface) in production code and pass a controllable fake in tests
(see `Infrastructure/Time` in the unit test project).

```csharp
var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 1, 6, 12, 0, 0, TimeSpan.Zero));
var sut = new DiscountCalculator(timeProvider);
```

## Review checklist

- [ ] Name follows `Method_Scenario_ExpectedBehavior` (including `TestName` on each `[TestCase]`)
- [ ] Arrange / Act / Assert sections present and clean
- [ ] Exactly one Act
- [ ] No branching, loops or computed expectations
- [ ] No unexplained literals
- [ ] Stubs vs mocks named by role
- [ ] No real infrastructure, clock or randomness in unit tests
- [ ] No `[SetUp]`-shared state; helpers instead
- [ ] Only public behavior is tested
- [ ] `dotnet test` was run and its summary reported
