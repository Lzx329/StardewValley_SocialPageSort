# Social Page Sort

A small SMAPI mod for Stardew Valley 1.6 that adds a sort button to the social page, so you can order villagers by friendship, name (Chinese pinyin), or upcoming birthday.

## Features

- Adds a sort button to the far left of the social page tab bar.
- **Friendship** — sorts by heart level, then by exact friendship points. Default: descending.
- **Name** — sorts Chinese names by pinyin (zh-CN collation); non-Chinese names are listed after them in alphabetical order. Default: ascending.
- **Birthday** — sorts by days until the next birthday (0 = today). Default: ascending.
- Your farmer is always listed first, and villagers you have not met always come after those you have met.
- The selected mode and direction are saved to `config.json` and restored on the next launch.
- Integrates with Generic Mod Config Menu when it is installed (optional).

## Requirements

- Stardew Valley 1.6
- SMAPI 4.0.0 or later

## Installation

1. Install SMAPI 4.x.
2. Unzip the release package and put the `SocialPageSort` folder into the game's `Mods` folder.
3. Launch the game. The SMAPI log should show `Social Page Sort 1.0.0`.

The mod folder must contain `manifest.json`, `SocialPageSort.dll`, and the `i18n` folder.

## Usage

1. Press `E` to open the menu and switch to the Social tab.
2. Click the sort button at the left of the tab bar (shown as a three-line icon when space is tight).
3. Click a mode name to apply it with its default direction, or click the arrow to flip the direction.
4. Click outside the panel to close it. Changes apply immediately and are saved.

## Configuration

`config.json` is created in the mod folder on first run:

```json
{
  "SortMode": "Friendship",
  "FriendshipDescending": true,
  "NameDescending": false,
  "BirthdayDescending": false
}
```

`SortMode` accepts `Friendship`, `Name`, or `Birthday`. A missing or invalid `config.json` falls back to friendship descending with a warning in the log.

## Notes

- Name sorting always keeps non-Chinese names after Chinese ones; changing the direction only reverses the order inside each group.
- Friendship order is close to, but not identical to, the vanilla order when two villagers have the same heart level and points.
- The mod only reads social page data and reorders the list. It does not modify NPC data or save files.
- The sort button is mouse-only for now; gamepad and keyboard navigation are not supported.

## Building from source

Requires the .NET 6 SDK and a local Stardew Valley installation with SMAPI.

```
dotnet build src/SocialPageSort.csproj
```

`GameDir` defaults to `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley`. Override it with `-p:GameDir="<path>"`. The built DLL is written to the repository root, while `bin`/`obj` go to `%TEMP%\SocialPageSort-build`.

## Project layout

- `src/` — C# source and the project file
- `i18n/` — runtime translations (copied from `src/i18n` during the build)
- `docs/` — requirements spec and delivery notes (Chinese)
- `release/` — release notes and the packaged zip

## License

No `LICENSE` file is included yet, so all rights are reserved until one is added.
