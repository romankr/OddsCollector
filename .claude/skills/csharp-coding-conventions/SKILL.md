---
name: csharp-coding-conventions
description: C# coding conventions for OddsCollector, adapted from Microsoft's .NET C# coding conventions and reconciled with the repo's .editorconfig. Use whenever you write, change, or review any C# code in OddsCollector.
---

# C# coding conventions

Based on Microsoft Learn, ".NET Coding Conventions"
(https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions),
restated for this repo (.NET 10, nullable enabled, Azure Functions isolated worker).

**Precedence:** the repo's `.editorconfig` and analyzers win over this skill; this skill wins over personal habit.
Match the style of surrounding code when neither says anything. The Microsoft article targets docs samples,
so a few of its rules (65-char lines, comment wording aimed at readers without an IDE) are relaxed below.

## General

- Prefer modern C# features: file-scoped namespaces, primary constructors, records, `required`,
  collection expressions, pattern matching, raw string literals, target-typed `new()`.
- Use language keywords for types (`string`, `int`, `nint`), not `String`, `Int32`.
- Prefer `int` over unsigned types unless the domain needs unsigned.
- Clarity over cleverness: no convoluted one-liners.
- `async`/`await` for all I/O; pass `CancellationToken` through. No `.Result` / `.Wait()`.

## Exceptions

- Catch only what you can handle. Never catch bare `Exception` without a filter
  (`catch (Exception e) when (e is JsonException or ArgumentException)`).
- Throw specific exception types with a meaningful message; use `ArgumentNullException.ThrowIfNull`
  and similar guard helpers (see `Models/Guard.cs`).
- Rethrow with `throw;` to keep the stack trace.
- Use `using` declarations (`using var x = ...;`) instead of `try`/`finally` that only disposes.

## Strings

- Interpolation (`$"{home} vs {away}"`) for short concatenation; `StringBuilder` for building in loops.
- Raw string literals (`"""..."""`) instead of escapes or verbatim strings for JSON and multi-line text.
- Logging is the exception: use message templates with named placeholders
  (`logger.LogInformation("Skipped event {Id}", id)`), never interpolation.

## Types, constructors, initialization

- Records: PascalCase primary constructor parameters. Classes/structs: camelCase.
- Use `required` properties to force callers to set values instead of long constructor parameter lists.
- Object initializers instead of setting properties one by one after `new`.
- Target-typed `new()` or `var x = new T()` when the declared type equals the created type.
- Collection expressions for all collections: `List<Odd> odds = [];`, `string[] keys = ["a", "b"];`.
- Default to `internal sealed` for classes that aren't meant to be public or inherited.

## `var`

The repo's `.editorconfig` permits `var` everywhere. Use it when the type is evident from the right-hand side
(`new`, cast, literal) or the type is anonymous/long generic (LINQ). Write the type explicitly when it isn't
apparent and matters to the reader (`int count = Convert.ToInt32(raw);`). Don't encode the type in the name
(`oddsList`, `countInt`) — name by meaning.

## Delegates, events, statics

- Use `Func<>` / `Action<>` instead of custom delegate types.
- Lambdas for handlers that are never unsubscribed; a named method when it must be removed later.
- Access static members through the declaring type, never through a derived type.

## Operators

- `&&` / `||`, never `&` / `|`, for boolean conditions.
- Parenthesize mixed arithmetic/relational/logical expressions for clarity
  (matches `dotnet_style_parentheses_*: always_for_clarity`).
- When wrapping at a binary operator, put the operator at the start of the new line.

## LINQ

- Meaningful names for query results (`upcomingEvents`, not `q`/`result`).
- Filter (`Where`) as early as possible.
- Pascal-cased, unambiguous names in anonymous projections (`HomeTeam`, `AwayTeam`, not two `Name`s).
- `var` for query results.

## Namespaces and usings

- File-scoped namespace, one per file, matching the folder path.
- `using` directives at the top of the file, outside the namespace, `System.*` first
  (`dotnet_sort_system_directives_first`). Shared imports go in `GlobalUsings.cs`.

## Layout and formatting

- Four spaces, no tabs; UTF-8 with BOM; final newline (per `.editorconfig`).
- Allman braces (opening brace on its own line); always use braces for `if`/`else`/loops.
- One statement and one declaration per line; a blank line between members.
- Keep lines readable (around 120 chars in this repo); break long argument lists one per line.
- No `this.` qualification.

## Comments

- `//` comments on their own line, one space after `//`, starting with a capital letter.
- Explain *why* (constraints, trade-offs, surprising behavior), not *what* the code does.
- XML doc comments (`///`) on public and non-obvious members; `<remarks>` for background.

## Tooling

- Run `dotnet build` with analyzers enabled and fix new warnings; don't suppress without a comment saying why.
- Use `dotnet format` to apply `.editorconfig` before committing when formatting changed.
- Follow the .NET Secure Coding Guidelines: no secrets in code or config committed to the repo
  (use Key Vault references / app settings).

## Review checklist

- [ ] File-scoped namespace; usings outside it, sorted
- [ ] Keywords for built-in types; `var` only where the type is clear
- [ ] Collection expressions, object initializers, target-typed `new()` where applicable
- [ ] No bare `catch (Exception)`; `throw;` for rethrow; `using` declarations for disposal
- [ ] Async all the way with `CancellationToken`
- [ ] Log message templates, not interpolated strings
- [ ] Allman braces, braces always, one statement per line
- [ ] Comments explain why, on their own line
- [ ] `dotnet build` shows no new warnings
