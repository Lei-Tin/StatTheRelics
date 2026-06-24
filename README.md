# Stat The Relics

Displays live and historical stat counters for every relic in Slay the Spire 2. The mod auto-patches relic tooltips to append usage data and persists that data into run saves and history files.

Current version: `1.0.0`

## Usage and Installation instructions

- Play as normal. Hover a relic to see its tracked counters; run-history views also show the last saved snapshot with a header banner.

## Features

- Tracks per-relic counters (default: flashes) plus relic-specific metrics defined in generated stats classes.
- Dynamically patches all relic obtain/activation/flash/tooltip methods via Harmony. 
- Persists counters into run saves and run history with the current mod version; incompatible old snapshots are ignored and tooltips show a version mismatch message instead of stale data.

## How it works

- Startup: the mod initializer wires Harmony patches and registers every `BaseRelicStats` definition found in the assembly so each relic can format its tooltip entry.
- Tracking: every time a relic is obtained or its effect/flash methods run, the tracker increments counters keyed by relic type and method name; setters are tracked too for stateful relics.
- Tooltip injection: when a relic tooltip is built, the tracker appends formatted stats text (including a banner note when viewing run history) to the description.
- Persistence: save/load postfixes capture a versioned snapshot of counters alongside run saves and history, then restore compatible snapshots when entering a run or browsing run history so stats stay in sync.

## Development notes

- Add or tweak relic-specific formatting by creating/editing a `BaseRelicStats` subclass under [RelicStats/Generated](RelicStats/Generated) or [RelicStats](RelicStats).
- Dynamic patch hints live in `RelicTracker.RelicPatches` and include method name heuristics for obtains (`OnObtain`, `OnEquip`, constructors), effects (`Activate`, `OnUse`, setters), flashes, and tooltip builders.
- The mod image is `StatTheRelics.png` at the repository root. 