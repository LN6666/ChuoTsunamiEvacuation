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

## P4-B Planned Future Step

- Generate real shelter markers.
- Display real shelter metadata.
- Validate shelter entry/result flow.
- Add debug-only hazard fixture visualization.
- Do not replace the tsunami risk wall or gameplay failure rules.
