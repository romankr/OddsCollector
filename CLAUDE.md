## Tech Stack
- .NET 10
- Microsoft Azure

## Commands
- Build: `dotnet build`
- Test: `dotnet test --no-build`
- Run: `dotnet run --project OddsCollector.Functions/OddsCollector.Functions.csproj`

## Workflow Rules
- ALWAYS create a feature branch before making changes
- Run `dotnet test` after every implementation
- Keep commits atomic - one logical change per commit

## Build environment for Claude (Cowork)
This file is only instructions — it cannot grant access. Real build/test access depends on the session setup:

### Option A — cloud sandbox (Linux, Ubuntu 24.04)
1. Copy the repo sources into the sandbox (skip `bin/`, `obj/`, `.git/`, `.idea/`).
2. Install SDK from the Ubuntu archive: `apt-get install -y dotnet-sdk-10.0`
3. `dotnet restore` → `dotnet build` → `dotnet test --no-build`
- Requirement: the org network allowlist (claude.ai → Admin settings → Capabilities) must include `api.nuget.org` (and `*.nuget.org`) — otherwise restore fails.
- After making changes in the sandbox, copy only changed source files back to the repo.

### Option B — user's Windows machine
- Needs a shell on this computer in the session; run the Commands above from the repo root.

### Always
- Report the exact `dotnet test` summary (passed/failed/skipped); never claim tests pass without running them.
