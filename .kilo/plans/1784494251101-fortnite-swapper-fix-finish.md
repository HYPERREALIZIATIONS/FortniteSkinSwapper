# Fortnite Swapper — Fix + Finish Plan

## Context
A near-complete, polished WPF (C# / .NET 8) desktop app already exists in this repo for
**cosmetic-only, local `.pak` file swapping** of Fortnite cosmetics using the public
[Fortnite-API.com](https://fortnite-api.com) catalog. It has Home, Browse, Swap, Settings screens,
mapping/backup/restore services, logging, and xUnit tests.

The app is ~95% done but has **one compile-breaking bug** and several gaps vs. the user's request:
- `BrowseView.xaml` references `LockerLabelConverter` which is **not defined** → build fails.
- **No website / Discord links** anywhere (explicitly requested: "links to the website and Discord").
- `DisclaimerAccepted` is stored but **never enforced** before swapping.
- `Theme` setting (Light/Dark) is exposed but `Theme.xaml` is dark-only; no theme switch.
- Manifest requires admin (`requireAdministrator`); no graceful handling if launched non-elevated.
- Minor API robustness gaps (no User-Agent, no retry/timeout tuning).

Scope chosen by user: **Fix + finish requested features**, strictly cosmetic-file-only.
No anti-cheat, login, server, or gameplay changes — consistent with the existing disclaimer.

## Constraints / Non-goals
- Keep all changes focused on cosmetic `.pak`/`.sig` replacement.
- Do NOT add cheating, bypass, login, server calls beyond the read-only Fortnite-API, or gameplay edits.
- The bundled `sample.mappings.json` remains an illustrative placeholder; the app continues to refuse
  swaps when no real per-version mapping exists. No authoritative mapping data is generated here.

## Tasks

### 1. Fix the compile-breaking bug (blocker)
- Add `src/FortniteSwapper/Converters/LockerLabelConverter.cs` implementing `IMultiValueConverter`.
  Inputs: `(cosmeticId string, ownedIds ICollection<string>)`.
  Returns `"Remove from My Locker"` when `ownedIds` contains the id, else `"Add to My Locker"`.
  (`BrowseView.xaml` already wires it via `conv:LockerLabelConverter x:Key="LockerLabel"`.)
- Verify no other missing types referenced in XAML that aren't defined.

### 2. Add website + Discord links (explicit request)
- In `MainWindow.xaml` sidebar (and/or Home footer), add a "Links" group with two buttons/hyperlinks:
  - Website → `https://fortnite-api.com` (configurable constant).
  - Discord → a `discord.gg` invite placeholder (configurable constant in `Paths`/`AppSettings`).
- Add `OpenLinkCommand` to `MainViewModel` using `Process.Start(url)` (WPF `Hyperlink.NavigateUri`
  needs `RequestNavigate` handling). Keep URLs as constants so they're easy to change.

### 3. Enforce the disclaimer gate
- In `SwapService.ApplySwap` and `SwapViewModel.Apply()`, block the swap when
  `!_settings.Current.DisclaimerAccepted` with a clear message: "Please open Settings and accept the
  disclaimer before swapping." (`AppSettings.DisclaimerAccepted` already exists.)

### 4. Theme support (Light/Dark toggle)
- Add a `Light` palette set to `Theme.xaml` and a `DynamicResource`-driven switch.
  Simplest robust approach: apply a `ResourceDictionary` (Dark/Light) to `App.Resources` based on
  `Settings.Current.Theme`, centralized in an `IThemeService.ApplyTheme(string)` called from
  `App.OnStartup` and `SettingsViewModel.Save()` / theme change.
- Keep existing dark colors as default; Light theme keeps readable contrast and the danger accent.

### 5. Admin / elevation UX
- Detect elevation at startup (`WindowsPrincipal(WindowsBuiltInRole.Administrator)`).
  If not elevated, show a friendly non-blocking banner/MessageBox: "This app needs admin rights to
  write Fortnite's pak files. Right-click and choose 'Run as administrator'." The manifest already
  requests admin, so this is a graceful fallback message, not a relaunch.

### 6. Harden API + checks (low risk)
- Add a `User-Agent` header and `Accept: application/json` in `ApiClient`.
- Add a simple retry/short-backoff (1 retry) on transient `HttpRequestException`/timeouts so a flaky
  network doesn't show a scary error on first load. Keep the existing 30s timeout.

### 7. Validation
- This container has **no .NET SDK**, so builds/tests cannot run here. The implementer must run on
  Windows with .NET 8 + Windows Desktop workload:
  ```powershell
  dotnet restore FortniteSwapper.sln
  dotnet build FortniteSwapper.sln -c Release
  dotnet test FortniteSwapper.sln
  ```
- Confirm the app compiles (no missing `LockerLabelConverter`), launches, Browse loads cosmetics,
  Swap enforces disclaimer + mapping, and Restore reverses a swap (covered by existing tests).

## Files touched (summary)
- New: `Converters/LockerLabelConverter.cs`, `Services/IThemeService.cs` (+ impl), elevation helper.
- Edit: `MainWindow.xaml`, `HomeView.xaml`, `MainViewModel.cs`, `SwapService.cs`,
  `SwapViewModel.cs`, `SettingsViewModel.cs`, `Theme.xaml`, `ApiClient.cs`, `App.xaml.cs`.
- Constants for website/Discord URLs added in `Paths.cs` or `AppSettings`.

## Risks / Notes
- Mappings are version-specific and user-supplied; the app correctly refuses to swap without a match.
- Editing `.pak` files may violate Epic ToS / be flagged by anti-cheat — already disclosed in-app.
- Because the SDK isn't available in this environment, validation steps must run on a Windows host.
