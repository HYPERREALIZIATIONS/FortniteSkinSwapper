# Fortnite Swapper

A friendly Windows desktop app (C# / WPF) for browsing Fortnite cosmetics and swapping how a
cosmetic **looks** on your own PC — cosmetic-only, local file changes.

> ⚠️ **Read this first.** This tool edits Fortnite's local `.pak` files so one cosmetic appears as
> another **on your screen only**. Other players still see your real cosmetics. Modifying game files
> can violate Epic's Terms of Service and may be detected by Easy Anti-Cheat / BattlEye. Swaps may
> break after a Fortnite update. There is **no** server interaction, login, or gameplay/anti-cheat
> change. Use at your own risk and prefer a separate account. Not affiliated with Epic Games.

## What it does
- **Home** — status (install found?, version, mapping coverage) and quick actions.
- **Browse** — search/filter the public [Fortnite-API.com](https://fortnite-api.com) catalog; mark cosmetics you own ("My Locker").
- **Swap** — pick an owned source cosmetic and a target look, then apply (auto-backup + restore available).
- **Settings** — Fortnite install path, version, mapping import, logs, disclaimer.

## How the swap works (and its limits)
The swap maps one cosmetic's local `.pak`/`.sig` files to another's, inside
`FortniteGame\Content\Paks`. That mapping is **version-specific** and must be supplied by you/community
(see `src/FortniteSwapper/Data/mappings/sample.mappings.json` — it is an illustrative placeholder only).
The app **refuses to swap** when no mapping exists for the selected version.

## Build & run (Windows)
Requires **.NET 8 SDK** with the Windows Desktop workload.

```powershell
# from the repo root
dotnet restore FortniteSwapper.sln
dotnet build FortniteSwapper.sln -c Release
dotnet run --project src/FortniteSwapper
```

The app requests administrator rights (needed to write under `Program Files`). Close Fortnite before swapping.

## Tests
```powershell
dotnet test FortniteSwapper.sln
```

## Project layout
```
src/FortniteSwapper/        WPF app (Models, Services, ViewModels, Views, Converters, Data)
tests/FortniteSwapper.Tests/  xUnit tests for mapping / backup / swap logic
```
