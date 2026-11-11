## Tech Stack
- .NET 11 (Azure Functions isolated worker, project SDK `Azure.Functions.Sdk`)
- Microsoft Azure

## Commands
- Restore: `dotnet restore` then `dotnet restore src/OddsCollector.Functions` (Azure.Functions.Sdk restores its extensions project only when the function app is the restore entry point; otherwise the build warns AZFW0108)
- Build: `dotnet build --no-restore` (not `--no-incremental`: a rebuild cleans the extensions project)
- Test: `dotnet test --no-build`
- Unit tests only (no Docker / Azure Functions Core Tools needed): `dotnet test --no-build --filter "TestCategory!=Integration"`
- Run with the emulators (Docker, Azure Functions Core Tools): `dotnet run --project src/OddsCollector.AppHost`

## Workflow Rules
- ALWAYS create a feature branch before making changes
- Run `dotnet test` after every implementation
- Keep commits atomic - one logical change per commit

## Build environment for Claude (Cowork)
This file is only instructions — it cannot grant access. Real build/test access depends on the session setup:

### Option A — cloud sandbox (Linux, Ubuntu 24.04)
1. Copy the repo sources into the sandbox (skip `bin/`, `obj/`, `.git/`, `.idea/`).
2. Install SDK from the Ubuntu archive: `apt-get install -y dotnet-sdk-11.0` (if the Ubuntu archive does not have it yet, use the dotnet-install script; that needs `builds.dotnet.microsoft.com` and `dot.net` in the network allowlist)
3. `dotnet restore` → `dotnet build` → `dotnet test --no-build`
- Requirement: the org network allowlist (claude.ai → Admin settings → Capabilities) must include `api.nuget.org` (and `*.nuget.org`) — otherwise restore fails.
- After making changes in the sandbox, copy only changed source files back to the repo.

### Option B — user's Windows machine
- Needs a shell on this computer in the session; run the Commands above from the repo root.

### Always
- Report the exact `dotnet test` summary (passed/failed/skipped); never claim tests pass without running them.
