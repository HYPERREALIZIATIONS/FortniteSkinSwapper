# Fortnite Cosmetic Swapper (WPF) — Implementation Plan

## Goal & Scope
Build a Windows desktop app in C# / WPF that lets a normal user browse Fortnite cosmetics
(from the public Fortnite-API.com), pick a cosmetic they own, pick a look to swap it with,
and apply the change to local game files as safely as possible — with logging and undo/restore.

**Cosmetic file replacement ONLY.** No code injection, no anti-cheat bypass, no gameplay or
stat changes. Everything is client-side and local to the user's machine.

### User-confirmed decisions
- **Swap engine:** Pak mirroring driven by a version-keyed mapping DB (see below).
- **Safety posture:** Clear disclaimers + logging in Settings; no blocking consent gate.
- **Links:** No website/Discord links in the UI.

## Important Reality / Constraints (must be reflected in code & docs)
- Fortnite cosmetics are server-authoritative. Local swaps only change what **the user** sees;
  other players still see the user's real cosmetics.
- Modifying Fortnite game files can violate Epic's Terms of Service and may be flagged by
  Easy Anti-Cheat / BattlEye, even for cosmetic-only edits. The app stays client-side, never
  injects or tampers with anti-cheat, and documents this risk in Settings.
- Actual swap correctness depends entirely on accurate, **per-patch** mapping data. The app
  ships the mapping *format* + an illustrative sample, and **refuses to swap** when no mapping
  exists for the detected Fortnite version. The swap engine is data-driven so version changes
  are absorbed by updating the mapping, not the code.

## Tech Stack
- .NET 8, WPF, MVVM via `CommunityToolkit.Mvvm`.
- `HttpClient` + `System.Text.Json` for the API; on-disk cache of the full payload.
- Images cached from `cdn.fortnite-api.com`.
- Rolling file logger (simple custom or Serilog) to `%LocalAppData%\FortniteSwapper\logs`.
- App manifest / elevation: writing to Fortnite paks requires admin. Request elevation
  (requireAdministrator manifest or per-operation UAC prompt) before any file write.

## Project Structure
```
src/FortniteSwapper/                 # WPF app
  Models/        Cosmetic, CosmeticType, MappingFile, MappingEntry, SwapRecord, AppSettings
  Services/      ApiClient, CacheService, MappingService, SwapService,
                 BackupService, LogService, FortniteLocator
  ViewModels/    HomeViewModel, BrowseViewModel, SwapViewModel, SettingsViewModel
  Views/         Home, Browse, Swap, Settings + shared Styles/Resources
  Data/          mappings/sample.mappings.json
  App.xaml, MainWindow.xaml
tests/FortniteSwapper.Tests/         # xUnit: MappingService, SwapService, BackupService, FortniteLocator
```

## Screens / UX Flow
1. **Home** — Welcome card; status strip (Fortnite path detected? version? mappings coverage %);
   quick actions: Browse, Settings, Restore All. A small disclaimer card links to Settings.
2. **Browse / Search** — Search box + filters (type, rarity, set, series). Responsive grid of
   cosmetic cards (image, name, rarity, type). "Add to My Locker" toggle marks owned cosmetics
   (persisted locally — no Epic login is possible client-side, so "owned" is user-declared).
   Pulls from API with disk cache + manual Refresh.
3. **Swap** — Two-pane wizard: **Source** (must be in My Locker) → **Target** (any cosmetic).
   Step indicators: Select source → Select target → Review → Apply. Apply runs backup + pak
   mirror with a live status line + inline log panel. Clear success/error messaging.
4. **Settings** — Fortnite install path (auto-detect + manual), selected/auto version,
   Mappings manager (import JSON, add/edit/delete entry, coverage count), Logs location
   (open button), disclaimers (ToS / anti-cheat / local-only visibility), theme (light/dark).

## Data: Fortnite API
- `GET https://fortnite-api.com/v2/cosmetics/br?language=en` (no key required for cosmetics).
- Map fields: `id`, `name`, `description`, `type` (outfit / backpack / pickaxe / emote /
  glider / wrap / …), `rarity`, `set`, `series`, `images` (smallIcon / icon / featured).
- Cache full payload to disk; use cache when offline; Refresh re-pulls.

## Data: Mapping DB (critical, version-keyed)
Format (`Data/mappings/sample.mappings.json`), documented in-app:
```json
{
  "version": "34.10",
  "cosmetics": {
    "<cosmeticId>": { "pak": "pakchunkNNN-Windows.pak", "sig": "pakchunkNNN-Windows.sig" }
  }
}
```
- Ship an **illustrative** sample only (clearly marked non-authoritative); the user/community
  supplies the real per-version mapping.
- `MappingService` loads the mapping for the selected version. If missing, browsing still works
  but **swap is blocked** with a clear message: "No mapping for version X — update mappings in
  Settings."
- Mappings UI: import JSON, add/edit/delete an entry, show coverage count.

## Swap Engine (SwapService + BackupService)
`FortniteLocator`: default `C:\Program Files\Epic Games\Fortnite`; verify
`FortniteGame\Content\Paks` exists. Detect version best-effort from the game exe version
resource, with manual override in Settings.

`ApplySwap(sourceId, targetId)`:
1. Validate both mapping entries exist and the pak/sig files exist on disk.
2. `BackupService` copies source `pak`+`sig` to
   `%LocalAppData%\FortniteSwapper\Backups\<timestamp>\` and records a `SwapRecord` in
   `appliedSwaps.json` (source, target, version, timestamp, backupPath). Verify backup succeeded
   before overwriting originals.
3. Mirror: copy target `pak` → source `pak` filename, target `sig` → source `sig` filename in the
   Paks folder, using atomic temp-file + rename.
4. Log every step (info/warn/error) to file + UI panel.

`Restore(record | backupPath)`: copy backup `pak`+`sig` back into Paks; remove record from
manifest. **Restore All** reverts every applied swap.

**Pre-flight safety checks:** valid path; version matches mapping; admin rights; Fortnite
process not running (detect + refuse if running); sufficient disk space. Abort + rollback on any
failure with a clear, actionable error.

## Logging
`LogService` writes timestamped entries to a rolling file; Swap screen shows a live log panel;
Settings has an "Open logs" button. Logs include the ToS/anti-cheat disclaimer reminder.

## Persisted Settings (`%LocalAppData%\FortniteSwapper\settings.json`)
`installPath`, `selectedVersion`, `theme`, `myLocker` (owned cosmetic ids), `logsPath`.

## Failure Modes & Handling
- API down → use cache; show error toast.
- Fortnite path not found → guide user in Settings.
- Missing mapping → block swap, instruct to update mappings.
- Not admin → prompt elevation.
- Game running → detect Fortnite process, refuse, instruct to close.
- Restore fails → alert, keep manifest, offer manual restore.

## Validation / Testing
- xUnit: `MappingService` parse/lookup; `SwapService` backup+restore logic against a fake Paks
  dir; `FortniteLocator` path detection; `BackupService` integrity.
- Manual smoke: launch → browse → add to locker → attempt swap with sample mapping in a test
  Paks dir → restore → verify originals restored.
- `dotnet format` + Release build before handoff.

## Open Questions
- Exact pak naming / mirror semantics differ by Fortnite version; the authoritative mapping
  source must be supplied/maintained by the user/community. App is data-driven to absorb this.
- Version auto-detection is best-effort; manual override available.
