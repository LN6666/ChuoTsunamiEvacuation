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

## Boundaries

- No Unity raw data processing.
- No raw GIS parsing.
- No PLATEAU reimport.
- No CityGML parsing.
- No `Chuo_BaseMap.unity` modification in early P4.
- No gameplay rewrite.
- No tsunami simulation.
- No hazard layer gameplay effect in P4-A.

## P4-A1 Planned Next Step

- Implement the real shelter loader.
- Support `sourceMode = test / real_sample`.
- Keep the default mode as `test`.
- Decide the Unity-readable copy path based on the P3 release contract.
- Add focused EditMode tests.

## P4-B Planned Future Step

- Generate real shelter markers.
- Display real shelter metadata.
- Validate shelter entry/result flow.
- Add debug-only hazard fixture visualization.
- Do not replace the tsunami risk wall or gameplay failure rules.
