# DeepSeek Review Prompt - P5 Final Closeout

You are DeepSeek V4 Pro performing the final Phase 5 closeout review for the Unity project `ChuoTsunamiEvacuation`.

## Review Goal

Review the entire Phase 5 integration, especially the combined P5-D, P5-E, P5-F, and P5-GH work, and decide whether the Phase 5 branch is ready to merge.

Branch:

`p5g-humanitarian-candidate-unity-integration`

Phase 5 scope:

Evidence-based evacuation building qualification, GIS routing outputs, and Unity integration. This is a prototype/research integration stage, not official navigation, not official route guidance, not a full real Chuo high-rise screening, and not a P6/P7 feature implementation.

## Files And Areas To Review

Review the git diff and relevant existing files, including:

- `Assets/Data/shelter_source_config.json`
- `Assets/Data/real_chuo_building_qualification.json`
- `Assets/Data/real_chuo_shelter_building_matches.json`
- `Assets/Data/real_chuo_osm_routes_sample.json`
- `Assets/Data/real_chuo_integrated_route_qualification.json`
- `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json`
- `Assets/Scripts/Data/P5CStaticDataLoader.cs`
- `Assets/Scripts/Data/RealQualifiedShelterDataLoader.cs`
- `Assets/Scripts/Data/P5DRoutePreviewTransformValidator.cs`
- `Assets/Scripts/Data/HumanitarianCandidateDataLoader.cs`
- `Assets/Scripts/Data/HumanitarianCandidateFeedbackFormatter.cs`
- `Assets/Scripts/Data/ShelterSourceConfigLoader.cs`
- `Assets/Scripts/Data/ShelterDataSourceResolver.cs`
- `Assets/Scripts/Gameplay/P5DRealQualifiedShelterRuntimeGenerator.cs`
- `Assets/Scripts/Gameplay/P5GHHumanitarianCandidateRuntimeGenerator.cs`
- `Assets/Scripts/Core/EvacuationGameManager.cs`
- `Assets/Scripts/Shelter/BuildingShelter.cs`
- `Assets/Scripts/Shelter/RealQualifiedShelterMetadata.cs`
- `Assets/Scripts/Shelter/HumanitarianCandidateMetadata.cs`
- `Assets/Tests/EditMode/P5CStaticDataLoaderTests.cs`
- `Assets/Tests/EditMode/P5DRealQualifiedGameplayDataTests.cs`
- `Assets/Tests/EditMode/P5GHHumanitarianCandidateDataTests.cs`
- `Assets/Tests/PlayMode/P5DRealQualifiedGameplayPlayModeTests.cs`
- `Assets/Tests/PlayMode/P5GHHumanitarianCandidatePlayModeTests.cs`
- `docs/P5_FINAL_CLOSEOUT_REVIEW.md`
- `docs/P5_WORKPLAN.md`
- `docs/PROGRESS_LOG.md`
- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`
- `docs/P5C_UNITY_INTEGRATION.md`
- `docs/P5E_ROUTE_RENDERING_QA.md`
- `docs/P5F_HIGHRISE_HUMANITARIAN_CANDIDATES.md`
- `docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md`
- `docs/UNITY_TESTING_WORKFLOW.md`

## Specific Review Questions

1. Safety boundaries:
   - Does `sourceMode` default remain `test`?
   - Does `real_qualified` remain opt-in?
   - Are `enableHumanitarianCandidates` and `enableLifeFirstCandidateSelection` default false?
   - Are humanitarian candidates double opt-in before they become selectable?
   - Are `Chuo_BaseMap.unity`, PLATEAU imported/raw files, `ProjectSettings`, `Packages`, raw/download/cache/tmp/.venv paths, and generated large data left untouched?

2. Data/runtime separation:
   - Does Unity runtime read only copied static JSON from `Assets/Data` for P5-C/P5-D/P5-GH?
   - Are runtime reads from `data_pipeline/raw`, `data_pipeline/downloads`, `data_pipeline/cache`, `data_pipeline/tmp`, `.venv`, and OSM caches rejected?
   - Are there any live routing, web request, scraping, OSMnx, or NetworkX runtime calls in Unity?

3. Official vs humanitarian candidate separation:
   - Are official/designated shelters kept separate from humanitarian emergency candidates?
   - Does the humanitarian loader accept only `candidateLayer = humanitarian_candidate`?
   - Are `candidateLayer = official` records skipped?
   - Are official-conflicting candidate records skipped safely?
   - Are humanitarian candidates never labeled as official shelters?
   - Do selectable humanitarian proxies keep `isOfficialShelter = false`?

4. Gameplay safety:
   - Does default test gameplay remain intact?
   - Does `real_qualified` only generate official/confirmed shelter proxies when explicitly enabled?
   - Do display-only candidate markers have no `BuildingShelter`, no `ShelterEntranceTrigger`, no collider, and no `Rigidbody`?
   - Does life-first selectable mode require both flags?
   - Does candidate status remain feedback only and not directly affect success/failure rules?

5. Route safety:
   - Does the parser identify GeoJSON-like `LineString` geometry?
   - Is the coordinate order `[longitude, latitude]`?
   - Is current CRS `EPSG:4326`?
   - Does route rendering remain fail-closed until a verified Unity/PLATEAU transform exists?
   - Are route distance/time feedback, estimated-route disclaimers, and OSM/ODbL attribution preserved?
   - Are unverified WGS84 route lines prevented from rendering?

6. Tests:
   - Confirm the GUI/headful automated test workflow is appropriate for this cloud Administrator environment.
   - Review whether EditMode and PlayMode tests cover the safety boundaries.
   - Latest closeout validation:
     - EditMode command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
     - EditMode XML: `test-results/editmode-results.xml`
     - EditMode result: 120 passed, 0 failed
     - PlayMode command: `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
     - PlayMode XML: `test-results/playmode-results.xml`
     - PlayMode result: 18 passed, 0 failed

7. Documentation:
   - Do docs clearly state Phase 5 is complete as a prototype/research integration stage?
   - Do docs clearly state `test` remains default, `real_qualified` is opt-in, humanitarian candidates are opt-in and non-official, and life-first selectable mode is double opt-in?
   - Do docs clearly state route rendering is fail-closed until transform validation?
   - Do docs avoid official navigation or official non-designated shelter claims?
   - Do docs state P5-F candidate data is a controlled/sample foundation, not full real Chuo high-rise screening?
   - Do docs state GUI/headful automation is the validated test path here?

8. Known limitations:
   - Are the remaining route-coordinate heuristic limitations, WGS84 span precision, `AllFinite` deduplication, QGIS recheck, and manual `real_qualified` smoke follow-ups documented at the right severity?

## Output Format

Please provide:

- Overall verdict: PASS, PASS WITH B-LEVEL FOLLOW-UPS, or BLOCKED.
- Any A-level blockers first.
- B-level follow-ups with file paths and concrete fixes.
- Confirmation of whether the Phase 5 branch is ready to merge.
- Confirmation that no P6/P7 features were implemented.

Do not modify files directly.
