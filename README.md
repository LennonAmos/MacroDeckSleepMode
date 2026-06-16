# MacroDeck Sleep Mode

Safe Macro Deck 2 plugin for Windows that switches to a dedicated animated sleep profile.

This local-development build opts out of Macro Deck's extension-store update check to avoid public-store 404 log errors before the plugin is published.

## What It Does

Adds these actions:

- `Toggle Sleep Mode`
- `Sleep Mode On`
- `Sleep Mode Off`
- `Go To Sleep Profile`
- `Wake From Sleep Profile`
- `Build Sleep Profile Layout`
- `Sleep Theme: Aurora`
- `Sleep Theme: Ocean`
- `Sleep Theme: Sunset`
- `Sleep Theme: Midnight`

## Recommended Setup

1. On your normal profile, put `Go To Sleep Profile` on your sleep button.
2. Optional: add one of the `Sleep Theme: ...` actions to change the background animation theme.

`Go To Sleep Profile` creates or refreshes a `Macrodeck Sleeping` profile with a 3x5 grid before switching to it. Every button in that sleep profile is a wake button, and every button uses a synchronized GIF tile from the selected local icon pack. Together they form a joined animated `Macrodeck Sleeping` layout.

When sleep is triggered, the plugin remembers the current profile for that device and switches to `Macrodeck Sleeping`. When any sleep profile button is pressed, it switches that same device back to the remembered profile.

Theme selection is saved in:

```text
%AppData%\Macro Deck\plugins\lenno.MacroDeckSleepMode\sleep_theme.txt
```

The matching themed icon packs are:

- `MacroDeck Sleep Aurora`
- `MacroDeck Sleep Ocean`
- `MacroDeck Sleep Sunset`
- `MacroDeck Sleep Midnight`

Creates and updates these variables:

- `{macrodeck_sleeping}`
- `{macrodeck_sleep_text}`
- `{macrodeck_sleep_frame}`
- `{macrodeck_sleep_theme}`
- `{macrodeck_sleep_tile_01}` through `{macrodeck_sleep_tile_15}`

## Safe Design

This plugin only creates or replaces the dedicated `Macrodeck Sleeping` profile layout. It does not rewrite your normal profiles or existing button actions.

The sleep profile uses pre-rendered GIF icons instead of live per-button icon updates, so Macro Deck is not asked to redraw 15 base64 images every frame.

## Install

1. Close Macro Deck.
2. Create this folder:

```text
%AppData%\Macro Deck\plugins\lenno.MacroDeckSleepMode
```

3. Copy the extension package files into it:

```text
MacroDeckSleepMode.dll
MacroDeckSleepMode.deps.json
ExtensionManifest.json
ExtensionIcon.png
README.md
LICENSE
```

4. Start Macro Deck.
5. Add `Go To Sleep Profile` to one normal-profile button.
6. Add one of the `Sleep Theme: ...` actions if you want a theme selector.

## Build

Requirements:

- Windows
- .NET 8 SDK
- Macro Deck 2 installed at `C:\Program Files\Macro Deck`

Build:

```powershell
dotnet build .\MacroDeckSleepMode.csproj -c Release
```

## Store Source Layout

The repository root should contain:

- `ExtensionManifest.json`
- `ExtensionIcon.png`
- `MacroDeckSleepMode.csproj`
- `Main.cs`
- `ToggleSleepModeAction.cs`
- `SleepModeOnAction.cs`
- `SleepModeOffAction.cs`
- `GoToSleepProfileAction.cs`
- `WakeFromSleepProfileAction.cs`
- `BuildSleepProfileLayoutAction.cs`
- `SetSleepThemeAction.cs`
- `SleepProfileController.cs`
- `SleepModeController.cs`
- `README.md`
- `LICENSE`

Do not commit `bin`, `obj`, built DLLs, or copied Macro Deck application DLLs.
