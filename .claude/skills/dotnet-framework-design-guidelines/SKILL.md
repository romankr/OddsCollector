---
name: dotnet-framework-design-guidelines
description: API and type design rules for OddsCollector, adapted from Microsoft's .NET Framework Design Guidelines (naming, types, members, extensibility, exceptions, collections, dispose). Use whenever you design, add, rename, or review types, members, interfaces, enums, exceptions, or collection-returning APIs in OddsCollector.
---

# .NET Framework Design Guidelines

Based on Microsoft Learn, "Framework design guidelines"
(https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/) and every sub-page
(naming, type design, member design, extensibility, exceptions, usage, common patterns).
The complete list of ~370 DO / CONSIDER / AVOID / DO NOT rules is in
[`references/guidelines-catalog.md`](references/guidelines-catalog.md) — open it when a question isn't covered below
or when reviewing a specific kind of member (operators, indexers, events, serialization, finalizers).

**Scope.** The guidelines target *reusable public libraries* and come from the 2008 book, so some rules are dated.
OddsCollector is an application (.NET 10, nullable enabled, Azure Functions isolated worker) where almost every type
is `internal`. Apply the rules to the *shape* of our internal APIs too — consumers are other classes and tests —
but use the repo adaptations below where the library-oriented advice doesn't fit.

**Precedence:** `.editorconfig` / analyzers → `csharp-coding-conventions` skill (style, syntax) → this skill
(API design) → personal habit. This skill decides *what* a type or member should be; the conventions skill decides
*how it's written*.

## Repo adaptations (where we deliberately differ from the book)

| Book rule | In OddsCollector |
|---|---|
| DO NOT seal classes without a good reason; unsealed is the default | Default to `internal sealed` (classes and records). Nothing here is designed for inheritance; sealing enables devirtualization and matches CA1852. Unseal only with a real extensibility need. |
| Prefer classes over interfaces; avoid interfaces with one implementation | Interfaces (`IUpcomingEventsClient`, `IOutcomePredictor`, converters…) are our DI and test seams — keep one interface per injectable service. Still: no marker interfaces, keep them small, and every interface must have an implementation **and** a consumer. |
| Use overloads rather than default (optional) arguments (CLS) | CLS compliance doesn't matter for internal code. Optional parameters are fine; avoid them on interface members (defaults are taken from the static type). Prefer overloads when defaults would change meaning. |
| Avoid `out`/`ref` parameters | Allowed only for the Try pattern (`bool TryX(..., out T result)`). Otherwise return a record or tuple. |
| Return `Collection<T>` / `ReadOnlyCollection<T>` | Return `IReadOnlyList<T>` / `IReadOnlyCollection<T>` / `IEnumerable<T>` (or arrays for small immutable results). Never expose `List<T>` / `Dictionary<,>` as a parameter or return type of a service interface. |
| Data Contract / XML / Runtime (`BinaryFormatter`) serialization | Use `System.Text.Json` only. `BinaryFormatter` is removed in modern .NET; don't add `[Serializable]`/`ISerializable`. |
| Dependency properties (WPF) | Not applicable. |
| Custom `EventHandler` delegates, `EventArgs` subclasses | We don't expose .NET events; use `Func<>`/`Action<>`, DI, or messages (Service Bus). |
| Basic Dispose Pattern with `Dispose(bool)` | Only when holding unmanaged resources (we don't). For managed disposables: `sealed` class + plain `Dispose()`/`DisposeAsync()`; prefer letting DI own lifetimes. No finalizers. |

## Naming

- PascalCase for namespaces, types, methods, properties, events, constants and public/static readonly fields;
  camelCase for parameters and locals. Two-letter acronyms are all caps (`IO`), longer ones Pascal (`Html`, `Json`, `Api`).
- Treat closed-form compounds as one word: `Id` (not `ID`), `Ok`, `Endpoint`, `Callback`, `Metadata`, `Email`,
  `Timestamp`, `Username` → `UserName`, `FileName`, `Canceled` (one *l*).
- Readability over brevity: no abbreviations (`GetWindow`, not `GetWin`), no Hungarian notation, no type in the name
  (`odds`, not `oddsList`), no names that differ only by case, avoid C#/VB keywords as identifiers.
- Types: nouns/noun phrases. Interfaces: `I` + adjective or noun (`IOutcomePredictor`); the default implementation is
  the same name without `I`. No `C` or other class prefixes. No `Base` suffix.
- Methods: verbs/verb phrases (`GetUpcomingEventsAsync`, `Convert`, `Predict`). Async methods end in `Async`.
- Properties: noun/adjective; booleans affirmative, `Is`/`Can`/`Has` only when it reads better (`IsCompleted`, not
  `NotFinished`). Collection properties are plural (`Bookmakers`, not `BookmakerList`). A property may share its
  type's name (`public Sport Sport`). Don't pair a property `X` with a `GetX()` method.
- Generic parameters: `T` when single and obvious, otherwise `T` + descriptive (`TResult`, `TEvent`).
- Required suffixes: `Exception`, `Attribute`, `EventArgs`, `Collection` (for `IEnumerable<T>` types), `Dictionary`,
  `Stream`, `Options` (for options classes — repo convention). Never `Enum`, `Flag(s)` or `Delegate` suffixes.
- Enums: singular name (`OutcomeType`); plural only for `[Flags]` (`MarketKinds`). No prefixes on values.
- Namespaces: `OddsCollector.Functions.<Feature>[.<Sub>]`, organized by feature/technology, matching folders.
  Never name a type the same as its namespace, and don't reuse common BCL names (`Task`, `Stream`, `Timer`,
  `Event`, `Message`, `Log`) — qualify them (`UpcomingEvent`, `EventResult`).
- New version of an API: similar name + meaningful suffix or new meaningful name. Never `Ex`/`2` suffixes unless
  no better name exists.

## Type design

- Each type is a cohesive set of related members — not a grab bag. Static classes only for stateless helpers,
  constants and extension methods (`Guard`, `UtcDateTime`, `ServiceCollectionExtensions`); never a misc bucket,
  never instance members.
- **class vs struct vs record:** default to `sealed class` for services and `sealed record` for immutable data
  (DTOs, models, messages). Use a `readonly record struct`/`readonly struct` only if it is a single logical value,
  under ~16 bytes, immutable and rarely boxed. Structs: no mutable state, `default` must be a valid value,
  implement `IEquatable<T>` (records do this for you).
- **Abstract classes:** only `protected` (or `private protected`) constructors; ship at least one concrete subtype.
- **Interfaces:** define one when value types and reference types must share an API or for DI seams; no marker
  interfaces (use an attribute); every interface needs an implementation and a consumer; adding members to an
  interface that others implement is a breaking change — prefer a new interface or a default implementation.
- **Enums:** use an enum instead of loose constants for closed sets (outcome, market type) and for parameters that
  would otherwise be multiple `bool`s. Don't use enums for open sets (bookmaker keys, sport keys from the API — keep
  those as strings/constants). Simple enums have a sensible zero value (e.g., `None`/`Unknown` or the most common
  default). `[Flags]` enums: powers of two, zero = `None`, add named combinations for common cases. No reserved,
  sentinel (`Max`, `Count`) or single-value enums. Keep `int` as the underlying type.
- **Nested types:** only for implementation details of the enclosing type; never public, never something callers
  instantiate or reference, never nested in an interface. Use namespaces for grouping.

## Member design

- **Overloads:** same parameter names and order across overloads; shorter overloads call the longest one; no
  overloads that differ only by `ref`/`out` or that change semantics at the same parameter position.
- **Properties vs methods:** a property is a cheap, side-effect-free field-like read. Use a method when the operation
  is slow, does I/O, returns a copy/snapshot, can throw, or returns different results on each call. Getters don't
  throw; no set-only properties; setter never more visible than getter; prefer `init`/`required` or get-only.
  Setters that throw must leave the old value intact.
- **Constructors:** do minimal work (capture dependencies, validate arguments) — no I/O, no async, no virtual calls.
  Parameters that set a property share its name (camelCase). Throw `ArgumentException`s from constructors when
  arguments are invalid. Use a static factory (`Create…`/`From…`) when construction semantics don't fit a
  constructor. Prefer static field initializers over static constructors; static constructors must not throw.
- **Fields:** all instance fields `private` (expose via properties). `const` only for values that never change
  (they are compiled into callers); `static readonly` for predefined instances. Never put a mutable object (array,
  `List<T>`, dictionary) in a `public`/`internal` `static readonly` field — use `ImmutableArray`, `FrozenSet`,
  `IReadOnlyList<T>` or a property returning a fresh copy.
- **Extension methods:** don't add them to types we own (use instance methods) or to `object`. OK for helpers over an
  interface (`IServiceCollection` registrations) or to avoid an unwanted dependency. Put them in the feature's
  namespace (e.g., `…OddsApi.Configuration`), not a generic `Extensions` namespace; no duplicate signatures.
- **Operators:** only on types that should feel primitive (value-like records/structs). Overload symmetrically
  (`==` with `!=`, `<` with `>`), keep `Equals`/`GetHashCode`/`==` consistent, never throw from `==`, no implicit
  conversions that can lose data or throw, no "cute" operators. Avoid overloading `==` on mutable reference types.
- **Parameters:**
  - Take the least derived type you need (`IEnumerable<T>` / `IReadOnlyCollection<T>`, not `List<T>`).
  - Validate arguments of every non-private member at the boundary: `ArgumentNullException.ThrowIfNull`,
    `ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentOutOfRangeException.ThrowIf…` or the repo `Guard` helpers.
    Validate enums explicitly (any int can be cast to an enum); don't rely on `Enum.IsDefined` for hot paths.
  - Two or more `bool` parameters → use an enum or an options record.
  - `out` parameters go after all by-value/`ref` parameters. In async methods `CancellationToken` is the last
    parameter (async methods can't have `out` — return a result type instead).
  - Keep parameter names identical between an interface and its implementations and across overrides.
  - `params` only when callers pass a few items inline; never mutate a `params` array; handle `null`.

## Designing for extensibility

- Choose the cheapest mechanism that meets the need; extensibility can be added later but never removed.
  Order of preference here: composition via DI (interfaces, `Func<>`), then options, then inheritance.
- Don't make members `virtual` without a concrete scenario; if needed, prefer `protected virtual` called from a
  non-virtual public member. Seal overrides.
- Treat `protected` members of an unsealed class as public (validate, document, version them).
- Callbacks: use `Func<>`/`Action<>` (not custom delegates); remember invoking a delegate runs arbitrary code.
- Abstractions must be proven by at least one real implementation and one consumer (plus a test fake) before they
  are introduced. No abstract base class whose only purpose is code reuse for us — use a private helper or
  composition instead.
- Never declare `protected` or `virtual` members on sealed types.

## Exceptions

- Report failures by throwing; never return error codes, `null`-as-error, or exception objects. A member that can't
  do what its name promises has failed.
- Don't use exceptions for normal control flow. For failures that are common and expected, provide a
  **Tester-Doer** (`CanX` / `X`) or **Try** pattern (`bool TryParse(string s, out T result)`, prefix `Try`, `bool`
  return, and keep a throwing counterpart). The Try method returns `false` only for its well-defined failure case
  and still throws for anything else.
- Throw the most specific standard exception: `ArgumentNullException` / `ArgumentOutOfRangeException` /
  `ArgumentException` (always with `paramName`; `value` for property setters), `InvalidOperationException` for wrong
  object state, `ObjectDisposedException` after dispose, `NotSupportedException` for unsupported operations.
- Never throw `Exception`, `SystemException`, `ApplicationException`, `NullReferenceException`,
  `IndexOutOfRangeException`, `AccessViolationException`, `StackOverflowException`, `OutOfMemoryException`,
  `ExecutionEngineException`, `COMException` or `SEHException`. Never derive from `ApplicationException`.
- Catch `Exception` only in top-level handlers (function entry points, middleware) or with an exception filter and
  rethrow (`throw;`). Never catch `StackOverflowException`.
- Don't throw from exception filters, `finally` blocks, `Dispose`, finalizers, equality operators, implicit
  conversions or property getters.
- Use throw helpers (`ThrowIfNull`, `Guard`, a private `static Exception CreateXException(...)`) to avoid duplicated
  throw code; it also keeps hot methods inlineable.
- Document (XML `<exception>`) exceptions that are part of a member's contract, and don't change them later.
- Custom exception: `sealed`, name ends in `Exception`, derive from the closest standard exception, provide the
  three standard constructors (none / message / message + inner). Only create one when callers handle it
  differently from existing types.
- Mind the cost: exceptions are slow; > ~100 throws/s is a design smell.

## Collections, arrays, XML

- Never expose `ArrayList`, `Hashtable`, `List<T>` or `Dictionary<TKey,TValue>` in a service interface or public
  model API; expose `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyDictionary<TKey,TValue>` or
  `IEnumerable<T>` (parameters: the least specialized type, usually `IEnumerable<T>`).
- Never return `null` for a collection — return an empty one (`[]`, `Array.Empty<T>()`).
- No settable collection properties (use `init` with collection expressions for DTOs that the serializer fills).
- Properties return live/cheap views; if you must copy (snapshot) or the source is volatile, make it a method.
- Prefer collections over arrays in APIs; arrays are fine for `byte[]` and small immutable results. Never a
  `readonly` array field exposed to others (its elements are still mutable). Jagged over multidimensional arrays.
- Don't return `IEnumerator<T>` except from `GetEnumerator`; don't implement both `IEnumerable<T>` and
  `IEnumerator<T>` on one type.
- Custom collection types are named `<Item>Collection` / `<Item>Dictionary`, with `ReadOnly` prefix for read-only
  variants; avoid implementation suffixes (`LinkedList`, `Hashtable`) on abstractions.
- XML (rare here): accept/return `XmlReader`, `XNode`/`XElement` — not `XmlDocument`/`XmlNode`.

## Attributes

- Name ends in `Attribute`; apply `[AttributeUsage]`; `sealed`; required arguments → constructor parameters with
  get-only properties of the same name; optional arguments → settable properties only; one constructor.

## Equality

- Overload `==` and `!=` together and keep them identical to `Equals`; override `GetHashCode` with `Equals`.
- Value types / records: value equality where meaningful (records already provide it — don't hand-roll it).
- Avoid overloading `==` on mutable reference types or when it would be much slower than reference equality.

## Dispose

- A type that owns `IDisposable`/`IAsyncDisposable` fields (that it created, not that were injected) must be
  disposable itself. Injected dependencies are disposed by the DI container — don't dispose them.
- `Dispose` is idempotent (safe to call twice), doesn't throw, and other members throw `ObjectDisposedException`
  afterwards (`ObjectDisposedException.ThrowIf(disposed, this)`).
- Don't make `Dispose()` virtual and don't add other `Dispose` overloads. Use `Dispose(bool)` +
  `GC.SuppressFinalize` only for unsealed types or ones wrapping unmanaged resources; wrap native handles in
  `SafeHandle` instead of writing finalizers. Never add a finalizer to a struct; never let a finalizer throw or
  touch other finalizable objects.
- Offer `Close()` only if it is domain terminology, implemented identically to `Dispose()`.

## Review checklist

- [ ] Names: PascalCase/camelCase correct, no abbreviations or Hungarian, `Id`/`Ok`-style compounds, required suffixes
      (`Async`, `Exception`, `Options`, …), no `Enum`/`Base`/`Ex` suffixes, no clashes with BCL type names
- [ ] Types: `internal sealed` by default; records for immutable data; struct only for tiny immutable values;
      static classes are cohesive and stateless
- [ ] Interfaces have an implementation and a consumer; no marker interfaces; no public nested types
- [ ] Enums instead of bool pairs/constants for closed sets; sensible zero value; `[Flags]` done right
- [ ] Properties are cheap and don't throw; expensive/I/O/snapshot work is a method
- [ ] Constructors only capture and validate; no I/O or virtual calls
- [ ] No non-private instance fields; no mutable objects in shared `static readonly` fields
- [ ] Parameters: least derived types, validated with `ThrowIf…`/`Guard`, `CancellationToken` last, names match
      across interface and implementation
- [ ] Collections: read-only interfaces in signatures, never `null`, no settable collection properties
- [ ] Exceptions: specific standard types with `paramName`; no error codes; Try pattern for expected failures;
      `catch (Exception)` only at entry points or with a filter + `throw;`
- [ ] Equality operators paired and consistent with `Equals`/`GetHashCode`
- [ ] Disposables owned → type is disposable; injected ones are not disposed
- [ ] `dotnet build` shows no new analyzer warnings (CA1000–CA2000 design/naming rules)
