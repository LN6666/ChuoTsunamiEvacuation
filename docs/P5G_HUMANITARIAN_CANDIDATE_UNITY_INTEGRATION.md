# P5-GH Humanitarian Candidate Unity Integration

## Status

P5-GH integrates the P5-F high-rise humanitarian candidate foundation into Unity with two explicit opt-in levels:

- Level 1: display-only humanitarian candidate markers.
- Level 2: life-first selectable humanitarian candidate mode.

Both levels are disabled by default. The committed `sourceMode` remains `test`.

## Runtime Data

Unity reads the copied static candidate sample:

- `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json`

The file is copied from the P5-F controlled fixture and is not full real Chuo high-rise screening. Runtime loading is fail-closed to approved `Application.dataPath + "/Data/"` paths and does not read `data_pipeline`, raw/download/cache/tmp/.venv paths, PLATEAU raw data, live routing, or web sources.

## Config Flags

`Assets/Data/shelter_source_config.json` now includes:

```json
"enableHumanitarianCandidates": false,
"enableLifeFirstCandidateSelection": false
```

Behavior:

- `enableHumanitarianCandidates = false`: no candidate markers; existing test, `real_sample`, and `real_qualified` behavior stays unchanged.
- `enableHumanitarianCandidates = true`: display-only humanitarian candidate markers appear and remain non-interactive.
- `enableHumanitarianCandidates = true` and `enableLifeFirstCandidateSelection = true`: only selected allowed candidates become playable life-first proxies. This is a double opt-in; the life-first flag alone creates no candidate markers or selectable proxies.

## Layer Rules

The loader accepts only `candidateLayer = humanitarian_candidate`. It skips `candidateLayer = official` records and records a diagnostic for the skipped layer. Humanitarian candidates are never mapped to official shelters.

Selectable life-first statuses are limited to:

- `humanitarian_strong_candidate`
- `humanitarian_candidate_with_review`

These remain non-official and warning-heavy. `humanitarian_weak_candidate`, `unknown`, and `not_recommended` stay display-only by default.

## UI Labels

Humanitarian candidate labels and result feedback include:

- This building is NOT officially designated as an evacuation shelter.
- Humanitarian emergency candidate
- Not officially designated
- Manual review needed when applicable
- Access uncertain when applicable
- Management agreement uncertain when applicable
- Seismic evidence uncertain when applicable
- Life-first scenario assumption
- Not a legal/public access guarantee
- Controlled P5-F sample data; not full real Chuo high-rise screening

Route/candidate evidence remains feedback only. OSM/ODbL attribution is preserved where route fields use OSM-derived prototype routing, and candidate route fields are not official evacuation routes.

## Gameplay Safety

Display-only markers have no `BuildingShelter`, no `ShelterEntranceTrigger`, no colliders, no `Rigidbody`, and no runtime shelter registration.

Life-first selectable proxies reuse the existing E-entry, climb, and ResultPanel flow, but:

- `isOfficialShelter = false`
- `sourceType = p5g_humanitarian_candidate`
- candidate status does not directly determine success/failure
- route, qualification, hazard, access, management, seismic, and candidate metadata are feedback only

Candidate selection remains feedback-only with respect to success/failure rules. Success/failure remains governed by existing shelter usability, climb, timing, risk, and anti-camping logic.

## Validation

GUI/headful automated tests passed:

- EditMode command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
  - XML: `test-results/editmode-results.xml`
  - result: 120 passed, 0 failed
- PlayMode command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
  - XML: `test-results/playmode-results.xml`
  - result: 18 passed, 0 failed

Forbidden path checks returned no output for:

- `Assets/Scenes/Chuo_BaseMap.unity ProjectSettings Packages`
- `data_pipeline/raw data_pipeline/downloads data_pipeline/cache tmp .venv`

## Boundaries

P5-GH does not modify `Chuo_BaseMap.unity`, PLATEAU imported files, `ProjectSettings`, or `Packages`. It does not implement live routing, web requests, flood simulation, NPC/crowd simulation, full real high-rise screening, or official shelter designation.

## Known Limitations

- Candidate data is controlled sample data only.
- No full real Chuo high-rise screening has been performed.
- Public access, management agreement, and seismic evidence remain uncertain for non-official candidates.
- Candidate route fields are prototype/supporting evidence only.
- UI is functional but not final visual polish.
