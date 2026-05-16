# Phase 4 Real Data Integration

## Goal

Integrate P3 validated real Chuo shelter and hazard sample outputs into the P2 Unity gameplay project while preserving existing test-mode gameplay, scenario rules, automated tests, and PLATEAU scene boundaries.

## Structure

- P4-A: branch setup, P3 merge, real shelter loader/sourceMode preparation.
- P4-B: real shelter marker gameplay integration and debug-only hazard fixture visualization.

## P4-A0 Status

Date: 2026-05-17

Branch: `phase4-unity-real-data-integration`

P3 branch merged: `origin/phase3-real-data-pipeline`

P3 latest commit merged: `ca74ab0` (`Add Phase 3 integrated shelter hazard pipeline prep`)

Merge commit: `ab6fe69`

Discovered P3 outputs:

- `data_pipeline/processed/real_chuo_shelters_sample.json`
- `data_pipeline/processed/real_chuo_shelters_sample.csv`
- `data_pipeline/processed/release/real_chuo_shelters_sample.json`
- `data_pipeline/processed/release/real_chuo_shelters_sample.csv`
- `data_pipeline/processed/release/sample_tsunami_hazard_zones.json`
- `data_pipeline/processed/release/p3_sample_release_manifest.json`

Supporting P3 docs now present:

- `docs/DATA_INTERFACE_CONTRACT.md`
- `docs/REAL_DATA_PIPELINE.md`
- `docs/REAL_SHELTER_DATA_SCHEMA.md`

Smoke test results:

- Unity EditMode command used: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode`
- Unity executable path: `C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe`
- Unity EditMode result: Unity launched and shut down, but did not produce `test-results/editmode-results.xml`.
- Editor log observation: batch startup/import activity was observed, but no completed test result XML was produced.
- Unity PlayMode result: skipped in P4-A0 because the same Unity batchmode environment issue would likely affect it.
- Manual Unity follow-up commands:
  - `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode`
  - `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode`
- P3 pipeline command used: `powershell -ExecutionPolicy Bypass -File data_pipeline/run_pipeline.ps1`
- P3 pipeline result: failed at step 1 because the active Python environment is missing `jsonschema`.
- P3 pytest availability: `python -m pytest --version` failed because `pytest` is not installed.
- P3 pytest result: skipped because pytest is unavailable without global installation.

Warnings and blockers:

- Unity automated test execution is an environment/test-launch warning for P4-A0, not a code blocker.
- Active Python environment is missing P3 pipeline dependencies: `jsonschema` and `pytest`.
- `docs/DATA_INTERFACE_CONTRACT.md` leaves the Unity-readable copy/load path, P3 ID to Unity shelter ID policy, WGS84-to-Unity placement, and marker binding decisions to P4. Resolve these in P4-A1 before implementing the loader.

## P4-A1 Status

Date: 2026-05-17

Files added or updated:

- `Assets/Data/real_chuo_shelters_sample.json`
- `Assets/Data/shelter_source_config.json`
- `Assets/Scripts/Data/RealShelterDataLoader.cs`
- `Assets/Scripts/Data/ShelterDataSourceResolver.cs`
- `Assets/Scripts/Data/ShelterSourceConfigLoader.cs`
- `Assets/Tests/EditMode/RealShelterDataLoaderTests.cs`

Source mode behavior:

- `sourceMode = test` remains the default.
- `sourceMode = real_sample` loads the copied Unity-readable sample from `Assets/Data/real_chuo_shelters_sample.json`.
- Unity runtime loading rejects `data_pipeline` paths and uses the copied `Assets/Data` file only.
- Missing or invalid real-sample loading logs a warning and falls back to test shelter data when `fallbackToTestOnError` is true.
- Existing `Assets/Data/test_shelters.json` behavior remains unchanged.

P3 sample root structure discovered:

- Top-level object, not a top-level array.
- Root fields: `dataset_id`, `generated_at`, `source_manifest`, `coordinate_reference_system`, `records`.
- No `schemaVersion` field.
- No separate `metadata` object.
- Shelter records are in `records`.
- Record fields include `id`, `name`, `type`, `latitude`, `longitude`, `address`, `capacity`, `floors_available`, `elevation_m`, `source`, `source_url`, `source_updated_at`, `notes`, and nested `unity`.
- `unity` fields are `prefab_hint`, `is_entry_enabled`, and `estimated_stair_floors`.
- `disasterTypes` is absent in the P3 release sample.
- `unityPosition` is absent in the P3 release sample.
- `capacity` can be null.
- `safeFloor` is absent; A1 maps `floors_available` / `unity.estimated_stair_floors` to the Unity-side `safeFloor`.

A1 boundary:

- A1 stops at data loading and source resolution.
- A1 does not generate shelter markers.
- A1 does not change scenes, gameplay rules, ResultPanel, tsunami risk wall logic, player movement, camera, PLATEAU imports, ProjectSettings, or Packages.
- P4-B can later consume `RealShelterDataLoader.RealShelterRecord[]` through `ShelterDataSourceResolver` to generate markers and display metadata.

Current Unity test status:

- Command attempted: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode`
- Result: Unity test launch failed with the same batchmode environment issue seen in P4-A0 and did not produce `test-results/editmode-results.xml`.
- Editor log observation: batch startup/import activity was available in the Editor log, but no completed test result XML was produced.
- PlayMode was not run for P4-A1 because EditMode launch did not complete.
- Manual follow-up command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode`

## Boundaries

- No Unity raw data processing.
- No raw GIS parsing.
- No PLATEAU reimport.
- No CityGML parsing.
- No `Chuo_BaseMap.unity` modification in early P4.
- No gameplay rewrite.
- No tsunami simulation.
- No hazard layer gameplay effect in P4-A.

## P4-A1 Completed Step

- Implemented the real shelter loader.
- Supported `sourceMode = test / real_sample`.
- Kept the default mode as `test`.
- Chose `Assets/Data/real_chuo_shelters_sample.json` as the Unity-readable copy path for the P3 release sample.
- Added focused EditMode tests.

## P4-B Status

Date: 2026-05-17

Files added or updated:

- `Assets/Data/sample_tsunami_hazard_zones.json`
- `Assets/Scripts/Data/ShelterGameplayDataMapper.cs`
- `Assets/Scripts/Data/ShelterDebugMetadataFormatter.cs`
- `Assets/Scripts/Data/TsunamiHazardFixtureLoader.cs`
- `Assets/Scripts/Data/TsunamiHazardDebugLayout.cs`
- `Assets/Scripts/Gameplay/RealShelterMarkerRuntimeGenerator.cs`
- `Assets/Scripts/Debug/TsunamiHazardDebugVisualizer.cs`
- `Assets/Tests/EditMode/RealShelterGameplayMappingTests.cs`
- `Assets/Tests/EditMode/TsunamiHazardFixtureLoaderTests.cs`

Real shelter marker behavior:

- `sourceMode = test` remains the committed default in `Assets/Data/shelter_source_config.json`.
- `sourceMode = test` keeps using the existing `test_shelters.json` and generated test shelter behavior.
- `sourceMode = real_sample` loads `Assets/Data/real_chuo_shelters_sample.json`, maps records into `ShelterDataLoader.ShelterData`, registers them as runtime shelters, and generates runtime-only real shelter markers.
- The generator uses existing `BuildingShelter` and `ShelterEntranceTrigger` components, so player entry, climb, success, failure, and active shelter entrance flow remain compatible.
- Existing test shelter scene objects are disabled at runtime only when `real_sample` is active, avoiding mixed test/real prompts without saving scene changes.
- The committed config must be reverted to `sourceMode = test` before commit if it is temporarily changed for manual validation.
- Unity runtime loading still rejects `data_pipeline` paths; real shelters are read only from the copied `Assets/Data` sample.

Real shelter positioning and metadata:

- If a real shelter has `unityPosition`, that position is used.
- The current P3 release sample has no `unityPosition`, so P4-B uses a deterministic schematic debug layout on the isolated test platform.
- P4-B does not convert lat/lon to PLATEAU coordinates, match roads, infer entrances, or fake geographic precision.
- Marker labels default to compact real sample metadata for readability: shelter name, facility type, capacity, and safe floor estimate where present.
- Press `M` in Play Mode when `real_sample` markers are active to toggle detailed shelter metadata, including address, source/source type, source updated date, and notes where present.

Hazard fixture behavior:

- The P3 hazard fixture was copied to `Assets/Data/sample_tsunami_hazard_zones.json` for Unity-readable loading.
- Hazard fixture root shape: top-level object with `dataset_id`, `generated_at`, `coordinate_reference_system`, `source`, and `zones`.
- `source` contains fixture/source metadata such as `source_id`, `source_family`, `source_name`, `official_status`, `source_updated_at`, and notes.
- `zones` contains zone records with `zone_id`, `zone_name`, `hazard_family`, `affected_zone`, `hazard_level`, `geometry_type`, GeoJSON-like `geometry`, inundation fields, tsunami height, and notes.
- The Unity loader reads zone metadata and geometry type. GeoJSON coordinates are not used for placement in P4-B.
- Hazard visualization is schematic debug-only. In Play Mode, press `H` to toggle the generated hazard fixture layer.
- Generated hazard shapes and labels have no gameplay colliders and no `EvacuationGameManager` callbacks.
- Hazard visualization does not affect success/failure, does not replace the tsunami risk wall, and does not implement flood simulation.

Manual validation plan:

1. Open Unity and confirm `Assets/Data/shelter_source_config.json` has `sourceMode = test`.
2. Enter Play Mode and confirm the existing test shelter markers and entry/climb/result flow still work.
3. Temporarily switch `sourceMode` to `real_sample` in `Assets/Data/shelter_source_config.json`.
4. Enter Play Mode and confirm five real sample shelter markers appear on the debug platform.
5. Confirm real shelter labels show names and metadata.
6. Press `T`, approach an enterable real shelter marker, press `E`, and confirm climb/result flow still works.
7. Confirm the blocked Ginza real sample marker remains not enterable.
8. Press `H` in Play Mode and confirm the schematic hazard fixture layer appears.
9. Confirm hazard visualization has no effect on success/failure and the tsunami risk wall behavior is unchanged.
10. Revert `sourceMode` to `test` before committing.

Current validation status:

- Focused EditMode tests were added for real shelter marker mapping, runtime lookup compatibility, metadata preservation, hazard fixture loading, schematic hazard mapping, and no gameplay rule effect.
- Command-line Unity validation command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode`
- Result: failed to produce `test-results/editmode-results.xml`, matching the known P4-A0/P4-A1 batchmode test-launch warning.
- First wrapper output: `Unity EditMode tests failed with exit code . Results: D:\UnityProjects\ChuoTsunamiEvacuation\test-results\editmode-results.xml`.
- Escalated retry also exited with code 1 and produced no `test-results/editmode-results.xml`.
- Manual Unity Editor Test Runner validation is still required before claiming the new EditMode tests passed in the Editor.

## P4-BV Automated Validation

Date: 2026-05-17

Automated coverage added or strengthened:

- `Assets/Tests/EditMode/RealShelterGameplayMappingTests.cs`
  - Confirms the committed `Assets/Data/shelter_source_config.json` still has `sourceMode = test`.
  - Confirms `real_sample` data maps to multiple marker-ready shelter records.
  - Confirms real shelter metadata is preserved for marker/debug UI display.
  - Confirms deterministic fallback positions are stable and separated enough for schematic debug markers.
  - Confirms metadata formatting handles missing optional fields without crashing.
  - Confirms runtime-registered real shelters can be resolved through the existing shelter lookup path.
- `Assets/Tests/EditMode/TsunamiHazardFixtureLoaderTests.cs`
  - Confirms the copied `Assets/Data/sample_tsunami_hazard_zones.json` fixture loads.
  - Confirms hazard name, level, source, notes, nullable depth/height, and schematic layout fields are handled safely.
  - Confirms hazard fixture loading uses the `Assets/Data` copy and rejects `data_pipeline` runtime paths.
  - Confirms generated hazard debug shapes are schematic, valid, unique, metadata-rich, and have no gameplay rule effect.
- `Assets/Tests/PlayMode/P4BDebugLayerPlayModeTests.cs`
  - Creates temporary runtime objects only.
  - Confirms the real shelter marker generator creates multiple `BuildingShelter` and `ShelterEntranceTrigger` objects with real metadata labels.
  - Confirms the hazard visualizer creates/toggles temporary collider-free debug objects.

Small production testability/safety change:

- `TsunamiHazardFixtureLoader.LoadFromPath` now rejects `data_pipeline` paths, matching the P4 runtime boundary already enforced for shelter source resolution.
- This does not change gameplay rules, scenes, ProjectSettings, Packages, PLATEAU assets, or tsunami risk wall behavior.

Manual visual validation still required:

- Confirm visible test-mode shelter gameplay in Unity Editor Play Mode.
- Temporarily switch `sourceMode = real_sample`, confirm real markers appear, and validate E entry/climb/result flow.
- Press `H` in Play Mode and confirm the hazard debug layer appears visually.
- Confirm the tsunami risk wall behavior and success/failure rules remain unchanged.
- Revert `sourceMode = test` before committing any future manual validation changes.

P4-BV CLI test status:

- Command attempted: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode`
- Non-escalated result: failed with `Unity EditMode tests failed with exit code . Results: D:\UnityProjects\ChuoTsunamiEvacuation\test-results\editmode-results.xml`.
- Escalated result: Unity launched, connected to licensing, then aborted because another Unity instance already had the project open.
- Exact Unity blocker: `It looks like another Unity instance is running with this project open. Multiple Unity instances cannot open the same project.`
- `test-results/editmode-results.xml` was not produced.
- PlayMode CLI was not run because EditMode CLI did not complete.
- Use the Unity Editor Test Runner for manual automated execution of the EditMode and PlayMode tests while the project is open in the Editor.

## P4-C Debug Label Readability Polish

Date: 2026-05-17

Readability changes:

- Real shelter marker labels now default to compact text: shelter name, facility type, capacity, and safe floor.
- Detailed shelter metadata remains available through the runtime `M` toggle for validation without crowding the default view.
- Real shelter label text is smaller and raised slightly above markers to reduce overlap.
- Hazard debug labels are compact by default: zone name, family, hazard level, and depth.
- Hazard debug text is smaller; the existing `H` hazard visualization toggle is unchanged.

Boundaries preserved:

- No gameplay rules changed.
- Hazard visualization remains debug-only and has no success/failure effect.
- No Unity scenes, `Chuo_BaseMap`, PLATEAU files, ProjectSettings, or Packages were modified.
- Default `sourceMode` remains `test`.

Validation:

- Focused tests were updated so compact labels exclude long address/source/notes fields by default, detailed shelter metadata can still be generated, hazard labels stay compact, and hazard debug still has no gameplay rule effect.
- Command attempted: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode`
- CLI result: failed with the known batchmode wrapper issue and did not produce `test-results/editmode-results.xml`.
- Manual recheck: in Unity Editor Play Mode, temporarily enable `real_sample`, confirm compact real shelter labels are readable, press `M` to toggle detailed shelter metadata, press `H` to toggle hazard visualization, and confirm gameplay success/failure rules are unchanged. Revert `sourceMode` to `test` after manual validation.

## Phase 4 Final Closure

Date: 2026-05-17

Branch: `phase4-unity-real-data-integration`

Latest implementation commit before final documentation: `65d8a8e` (`fix(p4): improve real data debug label readability`)

Phase 4 completed scope:

- P4-A0 established the Phase 4 branch, merged the Phase 3 real-data pipeline outputs, and documented the integration baseline.
- P4-A1 added Unity-side real shelter sample loading, source-mode selection, copied `Assets/Data/real_chuo_shelters_sample.json`, and kept default `sourceMode = test`.
- P4-B generated runtime-only real shelter markers from `real_sample` data, preserved existing shelter entry/climb/result flow, and added debug-only tsunami hazard fixture visualization.
- P4-BV added automated EditMode and PlayMode validation for source mode, shelter mapping, fallback layout, metadata, hazard loading/layout, and hazard no-gameplay-effect behavior.
- The fallback layout fix stabilized deterministic separated marker positions for real sample records without usable `unityPosition`.
- P4-C improved real shelter and hazard debug label readability while preserving detailed metadata access through the `M` toggle and hazard visualization through the existing `H` toggle.

Final data-source boundaries:

- The committed default remains `sourceMode = test` in `Assets/Data/shelter_source_config.json`.
- `sourceMode = real_sample` reads the copied Unity-readable shelter sample at `Assets/Data/real_chuo_shelters_sample.json`.
- Unity runtime shelter loading rejects `data_pipeline` paths.
- The hazard debug layer reads the copied Unity-readable fixture at `Assets/Data/sample_tsunami_hazard_zones.json`.
- Unity runtime hazard fixture loading rejects `data_pipeline` paths.
- No real GIS alignment, route finding, PLATEAU building matching, CityGML parsing, crowd simulation, or flood simulation was added.

Final validation:

- Unity Editor EditMode Test Runner `Run All`: passed.
- Unity Editor PlayMode Test Runner `Run All`: passed.
- Manual validation passed for default test gameplay.
- Manual validation passed for `real_sample` marker generation, compact/detailed metadata display, shelter entry, climb, and result flow.
- Manual validation passed for `H` hazard debug visualization toggle.
- Manual validation confirmed hazard visualization has no gameplay effect and tsunami risk wall behavior is unchanged.
- `sourceMode` was restored to `test` after manual validation.

Known warning:

- `tools/run_unity_tests.ps1` still has a batchmode/test-results XML reliability issue in this environment. Earlier CLI runs either failed to produce `test-results/editmode-results.xml` or were blocked by the project already being open in Unity Editor. This is an environment/tooling follow-up, not evidence of failing Unity Editor tests.

Ready for review:

- Phase 4 is ready for DeepSeek final review and a merge/completion decision, subject to any review findings.
