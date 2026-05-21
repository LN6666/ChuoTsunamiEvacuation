# DeepSeek Review Prompt: P5-GH Humanitarian Candidate Unity Integration

Review the current git diff for the ChuoTsunamiEvacuation Unity project on branch `p5g-humanitarian-candidate-unity-integration`.

## Merged Context

P5-D:
- Added opt-in `sourceMode = real_qualified`.
- Official/qualified Chuo shelters can become playable runtime shelter targets.
- Default `sourceMode` remains `test`.
- Route/qualification/hazard metadata remains feedback only.

P5-E:
- Added route geometry parsing and fail-closed WGS84 validation.
- Route geometry is EPSG:4326 GeoJSON-like `LineString` with `[longitude, latitude]` pairs.
- Route rendering remains disabled until a verified Unity/PLATEAU transform exists.
- Route distance/time and OSM/ODbL attribution remain available.

P5-F:
- Added data-only high-rise humanitarian candidate schema/rulebook/source plan/sample/tests/docs.
- `candidateLayer` separates `official` and `humanitarian_candidate`.
- `publicAccessStatus = unknown` does not hard-exclude but triggers manual review.
- Humanitarian candidates are not official shelters.
- Data is controlled sample data, not full real Chuo high-rise screening.

## P5-GH Scope Implemented

P5-GH adds:
- copied static Unity data: `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json`
- default-off config flags:
  - `enableHumanitarianCandidates = false`
  - `enableLifeFirstCandidateSelection = false`
- Unity candidate loader with Assets/Data-only path guard
- display-only humanitarian candidate marker layer
- life-first selectable candidate mode behind both flags
- metadata and ResultPanel feedback labels for non-official/manual-review/life-first warnings
- EditMode and PlayMode tests
- docs: `docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md`

## Key Files Changed

- `Assets/Data/shelter_source_config.json`
- `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json`
- `Assets/Scripts/Data/ShelterSourceConfigLoader.cs`
- `Assets/Scripts/Data/HumanitarianCandidateDataLoader.cs`
- `Assets/Scripts/Data/HumanitarianCandidateFeedbackFormatter.cs`
- `Assets/Scripts/Gameplay/P5GHHumanitarianCandidateRuntimeGenerator.cs`
- `Assets/Scripts/Shelter/HumanitarianCandidateMetadata.cs`
- `Assets/Scripts/Shelter/BuildingShelter.cs`
- `Assets/Scripts/Core/EvacuationGameManager.cs`
- `Assets/Scripts/Result/ResultMetrics.cs`
- `Assets/Tests/EditMode/P5GHHumanitarianCandidateDataTests.cs`
- `Assets/Tests/PlayMode/P5GHHumanitarianCandidatePlayModeTests.cs`
- `docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md`

## Validation

GUI/headful automated EditMode:
- command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- XML: `test-results/editmode-results.xml`
- result: 115 passed, 0 failed

GUI/headful automated PlayMode:
- command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- XML: `test-results/playmode-results.xml`
- result: 17 passed, 0 failed

Forbidden path checks returned no output:
- `git status --short -- Assets/Scenes/Chuo_BaseMap.unity ProjectSettings Packages`
- `git status --short -- data_pipeline/raw data_pipeline/downloads data_pipeline/cache tmp .venv`

## Safety Boundaries To Review

Confirm:
- `sourceMode` default remains `test`.
- `real_qualified` remains opt-in.
- `enableHumanitarianCandidates` defaults false.
- `enableLifeFirstCandidateSelection` defaults false.
- Humanitarian candidates are never labeled official shelters.
- `candidateLayer = official` records are skipped by the humanitarian loader.
- Humanitarian candidate proxies use `isOfficialShelter = false`.
- Display-only markers have no `BuildingShelter`, no `ShelterEntranceTrigger`, and no colliders.
- Life-first selectable candidates are limited to `humanitarian_strong_candidate` and `humanitarian_candidate_with_review`.
- Candidate status does not directly determine success/failure.
- Runtime loading is Assets/Data-only and rejects `data_pipeline/raw/download/cache/tmp/.venv` paths.
- No live routing, web requests, flood simulation, NPC/crowd simulation, scene edits, PLATEAU edits, ProjectSettings edits, or Packages edits were introduced.
- OSM/ODbL attribution remains preserved where route fields use OSM-derived prototype routing.

## Known Limitations

- Candidate data is controlled P5-F sample data only.
- Full real Chuo high-rise screening is future work.
- Public access, management agreement, and seismic evidence remain uncertain for non-official candidates.
- Candidate route fields are prototype/supporting evidence only.
- UI is functional but not final visual polish.

## Requested Review Focus

Prioritize:
- compile risks
- NullReferenceException risks
- Unity lifecycle issues
- layer-mixing or official/candidate labeling risks
- path-boundary mistakes
- tests that may give false confidence
- result feedback and prompt clarity
- whether life-first selectable mode remains clearly opt-in and non-official

Future follow-up:
- full real Chuo high-rise screening and stronger evidence collection for access, management agreement, seismic safety, and route provenance.
