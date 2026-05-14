# Unity Testing Workflow

## Purpose

Milestone 2-03 adds a lightweight Unity test foundation for the ChuoTsunamiEvacuation prototype.

The tests are intended to catch basic config/data regressions before larger gameplay-rule changes. They are not a full QA suite.

## Unity Test Runner

### EditMode

In Unity:

1. Open Window > General > Test Runner.
2. Select EditMode.
3. Run all EditMode tests.

EditMode tests cover config and data loading without entering Play Mode.

### PlayMode

In Unity:

1. Open Window > General > Test Runner.
2. Select PlayMode.
3. Run all PlayMode tests.

PlayMode tests are smoke tests only. They instantiate minimal test objects and do not open or modify Chuo_BaseMap.unity.

## PowerShell Runner

From the project root:

```powershell
.\tools\run_unity_tests.ps1 -Mode EditMode
.\tools\run_unity_tests.ps1 -Mode PlayMode
.\tools\run_unity_tests.ps1 -Mode All
```

Results are written to:

- test-results/editmode-results.xml
- test-results/playmode-results.xml

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

### Assets/Tests/PlayMode/EvacuationSmokePlayModeTests.cs

Covers:

- A minimal `EvacuationGameManager` can start without the PLATEAU scene.
- The manager reaches PreEvent state.
- Manual start method moves the manager to Playing state.
- Missing tsunami config fallback helper returns safe values in Play Mode.

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
