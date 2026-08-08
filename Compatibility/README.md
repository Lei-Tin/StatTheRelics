# Relic behavior fingerprints

These files contain normalized IL hashes for every relic model and its nested async state machines. They make behavior changes visible even when a game update keeps the same public method signature.

Export a baseline for the currently installed game build:

```powershell
dotnet run --no-restore --project .\tools\InspectRelics.csproj -- .\local.props --export-relic-fingerprint .\Compatibility\relic-fingerprint-v0.110.1.json
```

Compare the currently installed game build with a saved baseline:

```powershell
dotnet run --no-restore --project .\tools\InspectRelics.csproj -- .\local.props --compare-relic-fingerprint .\Compatibility\relic-fingerprint-v0.110.1.json
```

The comparison reports added and removed relics, member shape changes, and changed method implementations. A changed hash identifies code that needs review; it does not by itself mean the stat patch is broken.
