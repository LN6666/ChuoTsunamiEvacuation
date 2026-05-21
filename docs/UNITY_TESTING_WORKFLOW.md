# Unity Testing Workflow

## Purpose

Milestone 2-03 adds a lightweight Unity test foundation for the ChuoTsunamiEvacuation prototype.

The tests are intended to catch basic config/data regressions before larger gameplay-rule changes. They are not a full QA suite.

## Automated Unity Test Runner

Manual Test Runner clicking is not the primary validation path for this project.
Use the PowerShell wrapper so tests are launched automatically, bounded by a timeout,
and written to XML under `test-results`.

### GUI/headful fallback

The approved cloud-desktop fallback is GUI/headful automation. Unity opens without
`-batchmode`, but the tests are still started by `-executeMethod`; no Test Runner
clicking is required.

From the project root:

```powershell
.\tools\run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
.\tools\run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
.\tools\run_unity_tests.ps1 -Mode All -LaunchMode Gui
```

EditMode tests cover config and data loading without entering Play Mode.
PlayMode tests are smoke tests only. They instantiate minimal test objects and do
not open or modify `Chuo_BaseMap.unity`.

Results are written to:

- `test-results/editmode-results.xml`
- `test-results/playmode-results.xml`

The wrapper prints explicit `total`, `passed`, `failed`, `skipped`, and
`inconclusive` counts after each run. PlayMode runs may first write Unity's
internal XML to the user `LocalLow` test-results path; the wrapper automatically
copies that fresh XML into `test-results`.

### Batch launch mode

Batchmode remains supported when the cloud Unity licensing/package state allows
it:

```powershell
.\tools\run_unity_tests.ps1 -Mode EditMode -LaunchMode Batch
.\tools\run_unity_tests.ps1 -Mode PlayMode -LaunchMode Batch
.\tools\run_unity_tests.ps1 -Mode All -LaunchMode Batch
```

This cloud Administrator environment has shown batchmode instability where the
Unity Licensing Client fails and Package Manager registers 0 packages. When that
happens, Unity modules such as UI, Physics, and UIElements may be unavailable
before script compilation. The wrapper detects the Licensing Client plus
`Registered 0 packages` blocker in the Unity log and fails fast with the blocker
lines instead of waiting for the full timeout.

When batchmode is blocked, use the GUI/headful commands above as the approved
automation path.

If Unity is not found, set either:

- `$UnityExecutablePath` near the top of `tools/run_unity_tests.ps1`
- `UNITY_EXE` environment variable pointing to `Unity.exe`

The script also tries common Unity Hub install paths based on `ProjectSettings/ProjectVersion.txt`.

## Test Files

### Assets/Tests/EditMode/ConfigLoaderTests.cs

Covers:

- `tsunami_event_config.json` can be loaded.
- Missing tsunami config path returns safe non-null defaults.
- Default tsunami config keeps manual start enabled.
- Default tsunami config keeps random start disabled.
- Countdown and wall duration are positive.
- Invalid or partial config sanitizes to safe values.

### Assets/Tests/EditMode/ShelterDataLoaderTests.cs

Covers:

- `test_shelters.json` can be loaded.
- `test_shelter_001` exists.
- `test_shelter_001` is enterable by default.
- Unknown shelter IDs return `null` and do not create destructive fallback data.
- Unknown shelter behavior is explicit through warning logs.

### Assets/Tests/EditMode/AntiCampingConfigTests.cs

Covers:

- `anti_camping_config.json` can be loaded.
- Anti-camping is disabled by default.
- Pre-warning camping threshold is positive.
- Camped shelter blocking is true by default.

### Assets/Tests/EditMode/ResultExportServiceTests.cs

Covers:

- ResultMetrics export payload contains required fields.
- CSV export writes a header and one row.
- JSON export creates a valid export record.
- Missing optional fields are handled safely.
- run_logs-style output directories do not need to exist before export.
- Advice generation covers blocked shelter, camping block, late failure, crowding delay, and success cases.

### Assets/Tests/EditMode/ShelterSourceConfigLoaderTests.cs

Covers:

- shelter_source_config.json can be loaded.
- sourceMode defaults to test.
- missing or unknown source config falls back to test mode.
- the P4 hook does not change current test_shelter loading behavior.

### Assets/Tests/EditMode/P5CStaticDataLoaderTests.cs

Covers:

- copied P5-C/P5-B static JSON files under `Assets/Data` can be loaded.
- route records preserve estimated route labels and OSM/ODbL attribution.
- route geometry uses WGS84 `EPSG:4326` GeoJSON-like `LineString` data.
- missing, malformed, unsupported, or invalid route geometry fails safely.
- `data_pipeline` paths are rejected for Unity runtime loading.

### Assets/Tests/EditMode/P5DRealQualifiedGameplayDataTests.cs

Covers:

- committed/default source mode remains `test`.
- `real_qualified` can be enabled through source-mode config.
- only official confirmed statuses are selectable by default.
- candidate, unknown, and not-qualified statuses stay non-playable/debug-only.
- metadata and feedback preserve qualification, confidence, warnings, route distance/time, estimated prototype route note, and OSM/ODbL attribution.
- current WGS84 route geometry is rejected for rendering until a verified Unity/PLATEAU transform exists.

### Assets/Tests/EditMode/ScenarioPresetLoaderTests.cs

Covers:

- scenario_presets.json can be loaded.
- activeScenarioId remains default for commit-ready project data.
- scenario overrides remain in memory and do not rewrite base JSON files.
- multi-shelter scenario references are valid.

### Assets/Tests/PlayMode/EvacuationSmokePlayModeTests.cs

Covers:

- A minimal `EvacuationGameManager` can start without the PLATEAU scene.
- The manager reaches PreEvent state.
- Manual start method moves the manager to Playing state.
- Missing tsunami config fallback helper returns safe values in Play Mode.
- ResultMetrics export records can be created in Play Mode without starting the PLATEAU scene.

### Assets/Tests/PlayMode/P5DRealQualifiedGameplayPlayModeTests.cs

Covers:

- default test shelter entry/result flow still works.
- opt-in `real_qualified` runtime proxies generate without scene saves.
- generated real_qualified targets expose metadata, triggers, and result feedback.
- route line rendering remains zero for current unverified WGS84 transform.
- route, qualification, and hazard metadata do not affect gameplay rules.

## Manual Tests Still Required

The following remain manual for now:

- Full random warning gameplay timing.
- Anti-camping physical behavior around shelter trigger volumes.
- Tsunami risk-front bypass movement test.
- Shelter entrance risk-front failure during climbing.
- ResultPanel visual readability.

## Current Limitations

PlayMode tests are smoke tests only. They intentionally avoid:

- Chuo_BaseMap.unity
- PLATEAU imported data
- full scene setup generation
- camera/player movement automation
- complete gameplay scenario automation

More complete scenario automation should be added in a later milestone after gameplay rules stabilize.
