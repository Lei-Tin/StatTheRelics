# Stat The Relics

![Stat The Relics mod image](./StatTheRelics.png)

Stat The Relics displays live and historical stat counters for relics in Slay the Spire 2. It patches relic tooltips to append usage data, then persists that data into run saves and run history.

Current version: `1.0.3`

Steam workshop link: https://steamcommunity.com/sharedfiles/filedetails/?id=3750161122

## Features

- Tracks per-relic counters, with relic-specific metrics for effects that are more interesting than simple flash counts.
- Appends formatted stats directly to relic tooltips during a run.
- Restores saved stats when viewing run history.
- Stores sidecar data with the current mod version, and hides stale stats when sidecar data was written by an incompatible version.

## Usage

Install the mod, start or continue a run, and hover a relic to see its tracked stats. Run-history views show the last saved snapshot with a small banner note.

Relic Collection and other compendium-style views intentionally do not show run-specific stats.

## Translation

All text added to relic tooltips is stored under [localization](./localization). The mod reads `LocManager.Instance.Language` and loads the JSON file whose name matches the game's current language code. The checked-in [eng.json](./localization/eng.json) is the English fallback, so every English key initially maps to the same English value.

To add a translation, copy `eng.json` to a new file named after the game's language code, then change only the values and leave the English keys unchanged. For example, Simplified Chinese uses `localization/zhs.json`:

```json
{
  "Cards Drawn": "抽到的牌",
  "No stats are available for this relic": "该遗物暂无可用统计数据"
}
```

Keep the `{0}` and `{1}` placeholders in the version-mismatch message. Missing or blank entries fall back to the built-in English text. If the selected language file is missing or invalid, the mod loads `eng.json`; if that is also unavailable, it still remains usable in English.

The active language code is checked whenever a relic tooltip is formatted. Changing the language in the game settings therefore switches the mod translation immediately, without restarting the game. Builds copy the complete `localization` directory next to the mod DLL and include it in the release zip, so translators can add or replace language files without rebuilding the mod.

## Development

Add or tweak relic-specific formatting by editing a `BaseRelicStats` subclass under [RelicStats/Generated](./RelicStats/Generated) or [RelicStats](./RelicStats).

Dynamic patch hints live in `RelicTracker.RelicPatches` and include method name heuristics for obtains (`OnObtain`, `OnEquip`, constructors), effects (`Activate`, `OnUse`, setters), flashes, and tooltip builders.

The mod image source is [StatTheRelics.png](./StatTheRelics.png) at the repository root. Builds stage it into the generated Godot project as `StatTheRelics/mod_image.png`, so STS2 can load `res://StatTheRelics/mod_image.png` from the exported PCK.

## Build

You need to setup `local.props` on the root directory of this project to be able to build, it should contain the following:

```
<Project>
  <PropertyGroup>
    <!-- Paths -->
    <STS2GamePath>{Game Path}</STS2GamePath>
    <GodotExePath>{Godot.exe Path}</GodotExePath>
    
    <!-- Mod Metadata -->
    <ModName>StatTheRelics</ModName>
    <ModDisplayName>Stat The Relics</ModDisplayName>
	<ModDescription>Displays stats for the various relics found in the spire</ModDescription>
    <ModAuthor>LeiT</ModAuthor>
    <ModVersion>1.0.3</ModVersion>
  </PropertyGroup>
</Project>
```

Use:

```powershell
dotnet build
```

The build generates the Godot project metadata, exports the PCK, copies the DLL and manifest to the configured STS2 mod folder, and creates `StatTheRelics_v{$version}.zip`.
