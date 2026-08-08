# Stat The Relics

![Stat The Relics mod image](./StatTheRelics.png)

Stat The Relics displays live and historical stat counters for relics in Slay the Spire 2. It patches relic tooltips to append usage data, then persists that data into run saves and run history.

Current version: `1.0.4`

Steam workshop link: https://steamcommunity.com/sharedfiles/filedetails/?id=3750161122

## Features

- Tracks per-relic counters, with relic-specific metrics for effects that are more interesting than simple flash counts.
- Appends formatted stats directly to relic tooltips during a run.
- Restores saved stats when viewing run history.
- Stores the mod version and game build in each sidecar snapshot.
- Keeps older snapshots readable after a mod or game update by showing their stored JSON values under an archived-stats header.

## Usage

Install the mod, start or continue a run, and hover a relic to see its tracked stats. Run-history views show the last saved snapshot with a small banner note.

Relic Collection and other compendium-style views intentionally do not show run-specific stats.

## Development

Add or tweak relic-specific formatting by editing a `BaseRelicStats` subclass under [RelicStats/Generated](./RelicStats/Generated) or [RelicStats](./RelicStats).

Relic-specific behavior lives in one Harmony patch group per relic. Shared lifecycle hooks initialize zero-value counters when relics are obtained and provide the generic `Flashes` fallback for unknown or changed relic implementations.

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
    <ModVersion>1.0.4</ModVersion>
    <MinGameVersion>0.107.1</MinGameVersion>
  </PropertyGroup>
</Project>
```

Use:

```powershell
dotnet build
```

The build generates the Godot project metadata, exports the PCK, copies the DLL and manifest to the configured STS2 mod folder, and creates `StatTheRelics_v{$version}.zip`.

## Game Update Checks

`tools/InspectRelics.csproj` validates Harmony targets and checks that every game relic has a generated stats definition. The versioned files under [Compatibility](./Compatibility) also fingerprint normalized relic IL, including async state machines, so implementation changes can be reviewed even when method signatures stay the same.
