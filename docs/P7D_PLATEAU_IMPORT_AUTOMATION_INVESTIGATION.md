# P7-D PLATEAU Import Automation Investigation

## Result

Automation status: BLOCKED for autonomous P7-D execution.

The local PLATEAU SDK package exposes a programmable importer, but running the full requested high-detail import autonomously is not safe under the current project policy.

## APIs And Files Inspected

- `Packages/manifest.json`
- `D:\UnityPackages\PLATEAU-SDK-for-Unity-v4.2.0.3.tgz`
- `package/Runtime/CityImport/Import/CityImporter.cs`
- `package/Runtime/CityImport/Import/CityImportProcedure/GmlImporter.cs`
- `package/Runtime/CityImport/Config/CityImportConfig.cs`
- `package/Runtime/CityImport/Config/ConfigBeforeAreaSelect.cs`
- `package/Runtime/CityImport/Config/PackageImportConfigs/PackageImportConfig.cs`
- `package/Runtime/CityImport/Config/PackageImportConfigs/LODRange.cs`
- `package/libplateau/CSharpPLATEAU/Dataset/CityModelPackage.cs`
- `package/libplateau/CSharpPLATEAU/Dataset/DatasetSourceConfig.cs`
- `package/Editor/Window/Main/PlateauWindow.cs`
- `package/Editor/Window/Main/Tab/ImportGuiParts/ImportButton.cs`
- `package/Tests/EditModeTests/TestCityImporter.cs`
- `package/Tests/TestUtils/TestCityDefinition.cs`

## What Was Learned

The SDK has `PLATEAU.CityImport.Import.CityImporter.ImportAsync(CityImportConfig, IProgressDisplay, CancellationToken?, ...)`.

The SDK package tests show local import can be driven from code using:

- `DatasetSourceConfigLocal`
- `AreaSelectResult`
- `GridCodeList`
- `PackageToLodDict`
- `CityImportConfig.CreateWithAreaSelectResult`

The SDK UI entry point is `PLATEAU/PLATEAU SDK`.

## Exact Blockers

1. Full requested P7-D target categories are large before conversion.

   Current source size for the requested categories is approximately 6.49 GB:

   - buildings: 2.95 GB
   - roads: 405.75 MB
   - bridges: 306.19 MB
   - underground: 21.70 MB
   - city furniture: 504.38 MB
   - water: 41.88 MB
   - vegetation: 836.65 MB
   - relief: 485.69 MB
   - disaster risk: 340.13 MB
   - land use: 579.35 MB
   - urban planning decision: 11.39 MB

2. Local SDK import can create side-effect files beside source data.

   The SDK test cleanup removes generated files such as `packed_image_*.png`, `combined_map_mesh*.png`, and DEM map folders from the local test source tree. Running against `D:\PLATEAU_DATA\Chuo_2025_CityGML` risks touching raw PLATEAU data.

3. Copying the full source into `Assets/P7HighDetail/Imported/` to avoid raw-source mutation would introduce multiple gigabytes of large raw/generated assets into the Unity worktree without an approved Git/LFS/cloud archive decision.

4. The generated high-detail scene and material/texture assets may be too large for normal Git history and require manual archive/release strategy.

5. P7-D requires visual/renderer/profiler validation. An automated import without human Unity UI verification would still not prove category coverage, LOD coverage, or visual quality.

## Decision

P7-D must stop at manual PLATEAU SDK import. The blocker is not absence of any SDK API; it is unsafe autonomous execution under the current path, raw-data, large-asset, and validation constraints.

Manual Unity UI import is required before P7-D can complete final profiling and baseline approval.
