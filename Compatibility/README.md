# Relic behavior fingerprints

These files contain normalized IL hashes for every relic model and its nested async state machines. They make behavior changes visible even when a game update keeps the same public method signature.

The [supported-version table](../README.md#supported-versions) identifies the matching Mod release for each supported game version. `1.0.9` targets `v0.113.1`; the older `v0.110.1` fingerprint is a comparison baseline, not a claim that this DLL runs on that game build. Cross-version saved snapshots use the existing archived-value display path.

Export a baseline for the currently installed game build:

```powershell
dotnet run --no-restore --project .\tools\InspectRelics.csproj -- .\local.props --export-relic-fingerprint .\Compatibility\relic-fingerprint-v0.113.1.json
```

Compare the currently installed game build with a saved baseline:

```powershell
dotnet run --no-restore --project .\tools\InspectRelics.csproj -- .\local.props --compare-relic-fingerprint .\Compatibility\relic-fingerprint-v0.110.1.json
```

The comparison reports added and removed relics, member shape changes, and changed method implementations. A changed hash identifies code that needs review; it does not by itself mean the stat patch is broken.

The installed `v0.113.1` baseline is in `relic-fingerprint-v0.113.1.json`; the complete review against `v0.110.1` is in [relic-review-v0.113.1.md](./relic-review-v0.113.1.md).
