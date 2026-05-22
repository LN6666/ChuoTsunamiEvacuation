# Project Tasks

## Current Phase

The project has completed environment setup, PLATEAU SDK installation, local Chuo City Buildings / LOD1 import, the first playable prototype, Phase 3 real data pipeline preparation, and Phase 4 Unity integration of the P3 real shelter/hazard sample.

The first playable prototype has been Unity-tested, DeepSeek V4 Pro max-thinking reviewed, committed, and pushed to GitHub.

Current branch focus:

P7-0 begins the PBL7 high-detail city foundation. P7-0 is docs/tools/prompts only: reference review, LOD strategy, asset inventory protocol, benchmark protocol, automation guard scripts, two-Codex workflow planning, and DeepSeek review preparation. P7-0 must not modify Unity scenes, ProjectSettings, Packages, PLATEAU imports, gameplay scripts, or Assets/Data. Phase 5 and Phase 6 remain the stable behavior baseline: default `sourceMode = test`, `real_qualified` opt-in, humanitarian candidate flags default false, OSM routes estimated only, route rendering fail-closed until WGS84 to Unity/PLATEAU transform validation exists, navigation display-only, and NPCs non-blocking.

## Phase 7: High-Detail Chuo Asset Loading, LOD, Streaming, and Windows EXE Optimization

Status: P7-0 in progress. P7 has exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D. Do not create P7-E, P7-F, or P7-G.

Planned tasks:

- P7-0: Scope Freeze + Reference Review + Automation Foundation. Create docs/tools/prompts only; run `tools/p7/run_p7_preflight.ps1`; prepare DeepSeek review. In progress.
- P7-A: Chuo Asset Inventory + LOD / Area Selection. Command-line scanner and report scripts inventory local asset/source folders without importing assets; reports record file counts, extensions, likely PLATEAU categories, candidate LOD path/name indicators, large files, and benchmark candidate categories. Implemented in current P7-A work; pending review.
- P7-B: Small-Area High-Detail Benchmark + Underground / Bridge / Road Feasibility. Benchmark selected small areas before any full Chuo import; record Editor and Windows x64 metrics where available. Pending.
- P7-C: Streaming / Chunk Loading + Visual Quality + Performance Optimization. Implement only approved chunk/LOD/optimization work after benchmark evidence and a confirmed Markdown plan. Pending.
- P7-D: Windows EXE Profiling + P7 Final Closeout. Record Windows x64 profiling, final decision log, DeepSeek review, and closeout. Pending.

### P7-A Automation / Performance Prep (Codex B)

Status: Implemented.

Tasks:

- Add command-line benchmark record skeleton creation under `docs/p7_benchmark_records/`.
- Add Markdown benchmark record required-field validation.
- Add benchmark preflight orchestration on top of the base P7 preflight.
- Document benchmark automation, performance log schema, and Windows x64 EXE profiling readiness.
- Preserve docs/tools/prompts-only scope and avoid Unity tests unless Unity files change.

Automation requirement:

- Run `powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1` before P7-0 review/commit.
- P7-A/P7-B/P7-C/P7-D must run automated Unity tests whenever Unity code/assets/scenes are changed.
- P7-0 intentionally does not run Unity tests because it is docs/tools/prompts only.

DeepSeek review requirement:

- Use `deepseek_review_prompt_p70.md` for P7-0.
- DeepSeek must confirm P7-0 is docs/tools/prompts only, protected paths are untouched, no dependencies or gameplay changes were added, automation scripts exist and run, P7/P8/P9/P10 boundaries are clear, and P7 has only five stages.

## Phase 6: Navigation Guidance and NPC Evacuation Prototype

Status: P6-E final closeout is complete after GUI automated validation and final DeepSeek review. No dependency import, package change, ProjectSettings change, scene change, PLATEAU change, data-pipeline raw/download/cache/tmp/.venv change, source-mode default change, gameplay success/failure logic change, P6-F work, or P7 work is allowed in P6-E.

Planned tasks:

- P6-0: open-source navigation/crowd/evacuation reference review and technical selection. Done in documentation only.
- P6-A: lightweight player navigation guidance prototype under `Assets/Scripts/Navigation/` after a confirmed Markdown plan.
- P6-B: small NPC evacuation prototype under `Assets/Scripts/NPC/` and/or `Assets/Scripts/Simulation/` after a confirmed Markdown plan.
- P6-C: integrate P6-A and P6-B only after both are reviewed and tested independently.
- P6-D: final playable behavior validation using generated/runtime test harnesses only; no P7 work. Done.
- P6-E: final P6 review and closeout. Done. Do not create P6-F or later stages.

P6-E final closeout outputs:

- reran GUI/headful automated EditMode validation: 142 total / 142 passed / 0 failed / 0 skipped / 0 inconclusive
- reran GUI/headful automated PlayMode validation: 27 total / 27 passed / 0 failed / 0 skipped / 0 inconclusive
- confirmed `Assets/Data/shelter_source_config.json` still has `sourceMode = test`
- confirmed `enableHumanitarianCandidates = false` and `enableLifeFirstCandidateSelection = false`
- created `docs/P6_FINAL_CLOSEOUT.md`
- created `deepseek_review_prompt_p6e.md`
- updated `docs/TASKS.md`
- updated `docs/REVIEW_BACKLOG.md`
- completed final DeepSeek review: PASS, no A-level blockers; local report `review_reports/deepseek_review_20260522_010241.md`
- confirmed final P7 boundary: Full Chuo Asset Loading + Underground/Bridge + LOD Upgrade + Game Optimization
- did not create P6-F and did not start P7
- did not add gameplay features, Navigation/NPC mechanics, live routing, web requests, flood simulation, crowd simulation, scene wiring, or asset loading

P6-D implemented outputs:

- added generated behavior validation scripts under `Assets/Scripts/Simulation/`
- added EditMode validation/source-boundary tests for P6-D
- added PlayMode generated scenario coexistence tests for player navigation plus NPC movement
- created `docs/P6D_PLAYABLE_BEHAVIOR_VALIDATION.md`
- created `deepseek_review_prompt_p6d.md`
- validated EditMode GUI tests: 142 total / 142 passed / 0 failed
- validated PlayMode GUI tests: 27 total / 27 passed / 0 failed

P6-D scope notes:

- validates target awareness, NPC coexistence, NPC arrival, guidance distance trend, required warnings, non-blocking NPCs, and result-manager absence
- keeps navigation display-only and NPCs ambient/non-blocking
- keeps `Chuo_BaseMap.unity` wiring deferred
- does not start P7 full Chuo asset loading, underground/bridge assets, LOD upgrade, or optimization

P6-0 completed outputs:

- created `docs/P6_OPEN_SOURCE_REFERENCE_REVIEW.md`
- created `docs/P6_TECHNICAL_SELECTION.md`
- created `deepseek_review_prompt_p60.md`
- reviewed Unity NavMeshComponents, Unity AI Navigation, Recast Navigation, A* Pathfinding Project, JR-Morgan Crowd Evacuation Simulation, keijiro unity-crowd-simulation, Unity ECS samples, Unity ML-Agents, JuPedSim, and SebLague Pathfinding
- selected `reference_only` first for navigation/crowd dependencies and no package import without separate approval
- selected custom lightweight P6-A navigation UI first
- selected custom lightweight P6-B NPC movement and target selection first
- deferred NavMesh/AI Navigation package workflow adoption, A* import, Recast integration, DOTS/ECS, ML-Agents, social-force models, congestion physics, and real route line rendering

Scope boundaries:

- P6-A/P6-B must not modify `Packages`, `ProjectSettings`, Unity scenes, PLATEAU imports, `Chuo_BaseMap.unity`, raw PLATEAU data, or protected data-pipeline paths during prototype stage.
- P6-A/P6-B must not introduce live routing or runtime web requests.
- P6-A/P6-B must not change `sourceMode` defaults or gameplay success/failure rules.
- P6-A must always display `estimated prototype route / not official navigation` when showing route guidance or route feedback.
- P6-B NPCs must not affect player success/failure.
- Navigation/crowd references are engineering references only, not official evacuation guidance sources.

## Phase 5: Qualification, Routing, PLATEAU Matching, and Unity Integration

Status: P5-D completed the opt-in `real_qualified` gameplay source first, P5-E completed route geometry validation and fail-closed route-preview QA next, P5-F completed the data-only high-rise humanitarian candidate foundation, P5-GH integrates the controlled candidate sample into Unity behind explicit default-off flags, and P5 final closeout is ready for final DeepSeek review.

Planned tasks:

- P5-A0: workspace setup, documentation baseline, and open-source reference candidate registry. Done.
- P5-A1: official evidence source review and open-source reference decisions. Done.
- P5-A2: evacuation building qualification rulebook foundation. Done.
- P5-B1: controlled sample qualification/matching/routing pipeline using the P5-A2 schema. Done.
- P5-B2: real Chuo ingestion readiness, dependency/environment plan, CRS/QGIS QA plan, and P5-B3 execution plan. Done.
- P5-B3: project-local Python GIS environment helper and source provenance/fixture readiness. Done.
- P5-B4: official Chuo/Tokyo/GSI data ingestion, real Chuo building qualification, and local PLATEAU building matching. Done.
- P5-B5: OSM routing sample and integrated route-output planning/implementation. Done.
- P5-B ReviewPrep: record B4/B5 validation and prepare DeepSeek review prompt. Done.
- P5-B DeepSeek review: review P5-B data correctness, scope boundaries, CRS/routing assumptions, and P5-C readiness. Done. Verdict PASS, no A-level blockers.
- Future P5 data QA: refine broad-area / non-building place-name heuristics beyond current tokens such as `公園一帯`, `地区`, and `リバーシティ`.
- P5-C: Unity read-only integration for qualified buildings, estimated route metadata, confidence, warnings, and source-mode safety. Implemented; validation pending.
- P5-B: PLATEAU qualification/matching and GIS routing pipeline.
- P5-C: Unity integration for qualified buildings, routes, confidence, warnings, and decision feedback.
- P5-D: opt-in real_qualified gameplay source, runtime playable real shelter proxies, ResultPanel feedback, and verified route-preview fallback. Implemented in current worktree.
- P5-E: verified route geometry parsing, WGS84 transform validation gate, selected/limited route-preview safety, and real_qualified gameplay QA. Implemented in current worktree.
- P5-F: high-rise humanitarian vertical evacuation candidate rulebook, schema, source plan, controlled fixture, validation tests, and DeepSeek prompt. Implemented in current worktree.
- P5-GH: integrate P5-E route validation and P5-F humanitarian candidate foundation with display-only markers and explicit life-first selectable candidates without changing official shelter semantics or enabling unverified route rendering. Implemented in current worktree.
- P5 final review: Codex closeout validation summary, scope boundary confirmation, and final DeepSeek prompt complete. Final DeepSeek review and merge decision pending.

Scope boundaries:

- Do not confuse official confirmed evacuation buildings with non-official candidates.
- Do not confuse controlled prototype route estimates with official evacuation routes.
- Do not implement full real routing, full PLATEAU matching, data downloads, scraping, dependency installation, or Unity changes until the matching/routing milestone explicitly allows them.
- Keep QGIS as a manual QA option, not a Unity runtime dependency.

P5-B1 completed outputs:

- controlled shelter, building, and route fixtures under `data_pipeline/qualification/`
- controlled pipeline config with deterministic thresholds
- standard-library controlled sample build script
- generated controlled qualification JSON/CSV outputs
- focused controlled pipeline tests
- P5-B1 documentation and progress updates

P5-B2 completed outputs:

- dependency/environment plan for validation and future GIS packages
- real Chuo ingestion plan with staged source-family workflow
- CRS and QGIS QA plan for projected metric operations and visual spatial checks
- machine-readable planning JSON files
- standard-library planning tests
- no dependency installation, data download, Unity change, real routing, or real PLATEAU matching

P5-B3 completed outputs:

- P5-specific requirements file for project-local validation/GIS/routing dependencies
- rerunnable local virtual environment setup helper under `data_pipeline/`
- source provenance/license review template with non-approved placeholder entries
- controlled real-source fixture plan for P5-B4
- standard-library tests for environment/source readiness planning files
- no setup script execution, dependency installation, data download, Unity change, real routing, or real PLATEAU matching

P5-B4 completed outputs:

- official source manifest for Chuo/Tokyo/GSI shelter and evacuation-place inputs
- official shelter ingestion script and 31-record normalized official Chuo output
- real building qualification script using a limited local PLATEAU CityGML mesh subset
- schema-valid qualification JSON/CSV and shelter-building match JSON/CSV
- QGIS QA GeoJSON layers for shelter points, matched building footprints, match lines, and low-confidence/unmatched records
- focused pytest coverage for source ingestion, schema validation, official evidence rules, B4 routing boundaries, and QGIS layer creation
- validation passed in `data_pipeline/.venv`: 31 schema-valid records and 9 pytest tests

P5-B5 completed outputs:

- controlled route test origins under `data_pipeline/qualification/`
- OSMnx/NetworkX walking-route builder for B4 qualified shelter/building targets
- integrated route/qualification builder preserving B4 evidence and match fields
- processed route JSON/CSV/GeoJSON outputs under `data_pipeline/processed/routes/`
- integrated route/qualification JSON/CSV outputs under `data_pipeline/processed/qualification/`
- QGIS QA layers for route origins, route lines, and route failures
- pytest coverage for OSM route semantics, non-official route flags, integrated route fields, QA layers, and no raw/cache/download runtime references
- validation passed in `data_pipeline/.venv`: 135 available OSM route records, 0 failed routes, and 6 pytest tests

P5-C implemented outputs:

- copied P5-B static JSON outputs into `Assets/Data`
- added Unity P5-C read-only loaders for integrated qualification, route samples, building qualification, and shelter-building matches
- preserved qualification status, confidence, manual-review flags, warnings, route distance/time, route geometry metadata, route prototype labels, and OSM/ODbL attribution
- added conservative collider-free P5-C debug markers behind `sourceMode = real_sample` and `enableP5COverlay = true`
- route lines remain disabled for current WGS84 geometry because the project has no verified Unity/PLATEAU coordinate transform
- added concise P5-C ResultPanel feedback for non-test shelter sources, with unavailable fallback when no safe shelter mapping exists
- default `sourceMode = test` remains unchanged
- P5-C does not implement real-time routing, flood simulation, NPC behavior, or route/qualification/hazard-based success/failure rules

P5-D implemented outputs:

- added opt-in `sourceMode = real_qualified` while keeping default `sourceMode = test`
- loaded copied P5-B/P5-C static JSON from `Assets/Data` only
- generated runtime-only playable shelter proxies for 27 official/qualified records
- made only `official_confirmed` and `official_confirmed_with_review` records selectable by default
- kept candidate, unknown, and not-qualified records non-playable/debug-only
- reused existing E entry, stair-climb, success/failure, and ResultPanel flow
- appended P5-D real qualified feedback with qualification status, confidence, manual review flag, warnings, route distance/time, estimated prototype route label, and OSM/ODbL attribution
- parsed route geometry but rejected current WGS84 route lines because no verified Unity/PLATEAU transform exists
- preserved route/qualification/hazard information as feedback only; no gameplay success/failure rule depends on it

P5-E implemented outputs:

- inspected `Assets/Data/real_chuo_osm_routes_sample.json` route schema and documented it in `docs/P5E_ROUTE_RENDERING_QA.md`
- confirmed route records use `routeId`, `originId`, `shelterId`, `plateauBuildingId`, `routeDistanceMeters`, `estimatedTravelTimeSeconds`, and GeoJSON-like `geometry.coordinates`
- confirmed route geometry is `EPSG:4326` WGS84 `LineString` with `[longitude, latitude]` pairs
- refined parser behavior so missing, malformed, unsupported, or invalid WGS84 geometry fails safely without crashing route metadata loading
- strengthened route preview validation for WGS84 lon/lat order, broad Chuo bounds, collapse, and implausible span
- confirmed no verified WGS84-to-Unity/PLATEAU transform exists in current Unity runtime code
- kept selected estimated route line rendering safely disabled for current real data while preserving route distance/time feedback
- kept `sourceMode = test` as the committed default and `real_qualified` opt-in only
- GUI/headful automated validation passed: EditMode 107 passed / 0 failed; PlayMode 13 passed / 0 failed
- created `deepseek_review_prompt_p5e.md`

P5-F implemented outputs:

- documented life-first humanitarian emergency high-rise candidate screening in `docs/P5F_HIGHRISE_HUMANITARIAN_CANDIDATES.md`
- separated official/designated evacuation facilities from humanitarian emergency candidate high-rises
- defined statuses `official_confirmed`, `official_confirmed_with_review`, `humanitarian_strong_candidate`, `humanitarian_candidate_with_review`, `humanitarian_weak_candidate`, `unknown`, and `not_recommended`
- created JSON Schema, rulebook, source plan, and seven-record controlled sample fixture under `data_pipeline/qualification/`
- added pytest coverage for schema validation, official/humanitarian separation, non-official warning policy, manual review triggers, and unknown public access behavior
- created `deepseek_review_prompt_p5f.md`
- performed JSON syntax validation with system Python; focused pytest requires `pytest` and schema validation requires `jsonschema`, which are not installed in the system Python environment
- made no Unity gameplay, scene, `Assets/Data`, PLATEAU import, `ProjectSettings`, `Packages`, large download, raw/cache/tmp/.venv, or `sourceMode` changes

P5-GH implemented outputs:

- copied P5-F controlled sample data to `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json`
- added `enableHumanitarianCandidates = false` and `enableLifeFirstCandidateSelection = false`
- added Unity-side humanitarian candidate loader with Assets/Data-only path restrictions
- preserved `candidateLayer = humanitarian_candidate` and skipped official-layer records
- added display-only candidate markers with no `BuildingShelter`, no `ShelterEntranceTrigger`, and no colliders
- added life-first selectable candidate proxies behind both flags for `humanitarian_strong_candidate` and `humanitarian_candidate_with_review`
- kept `humanitarian_weak_candidate`, `unknown`, and `not_recommended` display-only
- kept life-first candidates non-official with `isOfficialShelter = false`
- added ResultPanel feedback for humanitarian emergency candidate, not officially designated, manual review, access/management/seismic uncertainty, life-first assumption, and controlled-sample limitation
- kept route/qualification/hazard/candidate status feedback-only for success/failure
- GUI/headful validation passed: EditMode 115 passed / 0 failed; PlayMode 17 passed / 0 failed
- created `docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md`
- created `deepseek_review_prompt_p5g.md`

P5 final closeout outputs:

- confirmed current branch and clean baseline before closeout docs
- confirmed P5-E and P5-F merge/content presence on the P5-GH branch
- confirmed key P5-D/P5-E/P5-F/P5-GH files exist
- confirmed safety boundaries: default `test`, opt-in `real_qualified`, default-off humanitarian flags, Assets/Data-only runtime reads, no live routing/web requests, no official navigation claims, and no success/failure rules based directly on route/qualification/hazard/candidate status
- confirmed route rendering remains fail-closed until verified Unity/PLATEAU transform validation exists
- confirmed P5-F/P5-GH candidate data is controlled/sample foundation data, not full real Chuo high-rise screening
- reran GUI/headful automated validation: EditMode 120 passed / 0 failed; PlayMode 18 passed / 0 failed
- created `docs/P5_FINAL_CLOSEOUT_REVIEW.md`
- created `deepseek_review_prompt_p5_final.md`

P5-B review prep completed outputs:

- recorded user-confirmed QGIS QA result for B4 shelter/building matching
- recorded user-confirmed QGIS QA result for B5 route origins and route lines
- created DeepSeek review prompt at `deepseek_review_prompt_p5b.txt`
- no code, Unity files, `Assets/Data`, raw/cache/download/tmp, or `.venv` files changed

## Milestone 0: Environment Setup

Status: Done

- Alibaba Cloud Tokyo cloud desktop created
- Unity installed
- Unity project created
- PLATEAU data downloaded
- PLATEAU SDK installed
- GitHub repository created
- Codex CLI installed
- DeepSeek review script added
- Chuo Buildings / LOD1 imported locally

## Milestone 1: Project Documentation

Status: Done

Tasks:

- Create AGENTS.md
- Create GAMEPLAY_SPEC.md
- Create TASKS.md
- Create ARCHITECTURE.md
- Create DATA_SCHEMA.md
- Create DECISIONS.md
- Create CODE_REVIEW_WORKFLOW.md

## Completed Milestone: First Playable Prototype

Status: Done

Goal:
Create and verify the first playable evacuation loop.

Completed features:

- Third-person player movement
- WASD / arrow-key movement
- Shift sprint
- Mouse-based third-person camera control
- E shelter entry
- Climb simulation
- T starts tsunami test
- Tsunami risk failure
- Failure during climb if tsunami reaches the active shelter entrance
- Isolated debug platform
- DeepSeek V4 Pro max-thinking review workflow

Acceptance Criteria:

- Unity Play test verifies movement, camera, shelter entry, climb, success, and failure.
- Countdown starts only after tsunami warning is triggered.
- Manual T tsunami warning start works for debugging.
- First playable test setup runs away from PLATEAU building geometry.
- DeepSeek review result is safe to commit.

## Milestone 2: Rules and Dataization 1.0

Status: In progress

Goal:
Move prototype rules and manually configured values into small, validated data/config structures while preserving the current first playable behavior.

### Milestone 2-01: Data-Driven Rules Integration

Status: Done

Completed:

- Added tsunami_event_config.json.
- Added test_shelters.json.
- Added anti_camping_config.json.
- Added GameConfigLoader.cs.
- Added ShelterDataLoader.cs.
- Added ResultMetrics.cs.
- Tsunami warning, countdown, and wall duration are JSON-driven.
- Shelter entry, climb time, crowding delay, and failure reason are JSON-driven.
- Anti-camping detection and blocking are config-gated and disabled by default.
- ResultPanel displays explainable result fields.
- Visible tsunami wall remains visual feedback and backup trigger detection.
- Gameplay failure uses tsunami risk-front / flooded-side logic.
- Unity Play testing completed.
- DeepSeek review found no A-level blocking issues after fixes.

### Milestone 2-02: Scenarioized Gameplay Rules 1.0

Status: Done

Completed:

- Added Assets/Data/scenario_presets.json.
- Added default, normal_success, random_warning, blocked_shelter, anti_camping, and late_failure scenarios.
- Added ScenarioPresetLoader.
- Added scenario-related EditMode tests.
- Added runtime assembly asmdefs needed by tests.
- Formalized random warning validation.
- Added soft-lock prevention when manualStartEnabled=false and randomStartEnabled=false.
- Formalized anti-camping behavior.
- Enhanced ResultPanel evacuation review text.
- Manual Unity scenario tests passed for default, random_warning, blocked_shelter, anti_camping, and late_failure.
- EditMode tests passed.
- PlayMode tests passed.
- DeepSeek review found no A-level blockers.
- Follow-up cleanup removed UTF-8 BOM from scenario_presets.json.

### Milestone 2-04: Multi-Shelter Decision Gameplay 1.0

Status: Done

Goal:
Upgrade the debug platform from a single-shelter loop into a multi-shelter decision field.

Completed:

- Expanded Assets/Data/test_shelters.json to five test shelter records.
- Added Near Official Shelter, Far Fast Shelter, Crowded Candidate Shelter, Slow Safe Shelter, and Blocked Test Shelter.
- Added real-data-ready optional shelter fields while keeping sourceType set to "test".
- FirstPlayableSceneBuilder now generates multiple shelter markers and entrance triggers from test_shelters.json.
- Shelter placement uses layoutPosition on the isolated debug platform.
- Shelter info UI shows name, ID, rank, official/candidate status, enterability, entry delay, climb time, crowding delay, and blocked failure reason.
- Scenario presets now make multi-shelter choices meaningful.
- ResultPanel review advice now reflects selected shelter decision context.
- Added/updated EditMode tests for multi-shelter data, scenarios, and generated setup.

Manual Unity scenarios tested:

- default
- random_warning
- blocked_shelter
- anti_camping
- late_failure

Review:

- DeepSeek review found no A-level blockers.

Automated test commands:

- .\tools\run_unity_tests.ps1 -Mode EditMode
- .\tools\run_unity_tests.ps1 -Mode PlayMode

Limitations:

- Debug platform only.
- No real Chuo facility import.
- No PLATEAU building matching.
- No real road or entrance navigation.
- P4 should integrate P3 processed real shelter data later.

### Milestone 2-05: Evaluation, Decision Feedback & Real-Data Integration Hooks 1.0

Status: Implemented; requires Unity automated/manual validation and DeepSeek review before commit.

Goal:
Make the multi-shelter serious-game prototype evaluable and exportable while preparing a minimal Unity-side hook for future P4 real-data integration.

Completed:

- Added ResultExportService.
- Added CSV and JSON result export under run_logs/.
- Added run_logs/ to .gitignore.
- ResultMetrics now includes runId, timestamp, and advice fields.
- Centralized next-step advice generation for blocked shelters, camping blocks, late risk failure, crowding delay, fast success, and fallback outcomes.
- EvacuationGameManager exports one result when success or failure is finalized.
- Added shelter_source_config.json with sourceMode defaulting to test.
- Added ShelterSourceConfigLoader.
- Added real_chuo_shelters_sample.example.json as an inactive P4 example stub.
- Added EditMode tests for export records, CSV/JSON output, safe missing fields, advice generation, and source-mode hook behavior.
- Added a PlayMode smoke test for creating an export record from ResultMetrics.

Manual Unity scenarios to validate:

- default
- blocked_shelter
- anti_camping
- late_failure
- export creation and cleanup

Automated test commands:

- .\tools\run_unity_tests.ps1 -Mode EditMode
- .\tools\run_unity_tests.ps1 -Mode PlayMode

Limitations:

- run_logs/ is local debug output only.
- No dashboard analytics.
- No real Chuo facility import.
- No coordinate conversion.
- No PLATEAU building matching.
- P4 will consume selected P3 output later; P2-05 does not read P3 data_pipeline outputs.

### Remaining Milestone 2 Work

Tasks:

- Move runtime data loading away from Application.dataPath before builds.
- Add editor validation support.

### Milestone 2-03: Unity Test Automation Foundation

Status: Done

Completed:

- Added EditMode tests for config/data loaders.
- Added PlayMode smoke tests that avoid Chuo_BaseMap.unity and PLATEAU data.
- Added PowerShell test runner.
- Added Unity testing workflow documentation.
- EditMode tests were executed successfully.
- PlayMode smoke tests were executed successfully.

Remaining:

- Record any DeepSeek review items in REVIEW_BACKLOG.md.

Acceptance Criteria:

- First playable behavior remains testable on the isolated debug platform. Done for 2-01.
- Tsunami timing, shelter rules, anti-camping settings, and result metrics are represented as data/config. Done for 2-01.
- Data loading validates required fields and logs clear errors. Partial; editor validation remains.
- Editor validation can detect missing or invalid milestone data.

## Milestone 3: Chuo Data Integration

Status: Started as Phase 3 real data pipeline work

Goal:
Connect the dataized gameplay loop to a small, controlled Chuo City shelter dataset without relying on inferred streets from Buildings / LOD1.

### Phase 3-00: Real Data Pipeline Scaffold + Schema + Validation Foundation

Status: Done

Completed:

- Added `data_pipeline/` scaffold.
- Added source manifest for synthetic sample data.
- Added real shelter export JSON Schema.
- Added synthetic sample raw shelter CSV.
- Added exporter for Unity-ready sample JSON/CSV.
- Added validator for schema compliance, unique IDs, Chuo/Tokyo coordinate sanity, and Unity interface fields.
- Added pytest coverage for schema, sample input, export behavior, validation, IDs, Unity interface fields, and coordinate ranges.
- Added one-command PowerShell runner.
- Added Phase 3 pipeline, schema, and data interface contract docs.
- Added `.gitignore` rules for raw/intermediate/cache/download/tmp folders and large GIS/archive files.

Scope:

- Does not modify Unity gameplay, Unity scenes, PLATEAU imported files, or existing `Assets/Data` gameplay JSON.
- Does not download PLATEAU data, re-import CityGML, parse CityGML, or integrate with Unity.

### Phase 3-01: Official Source Registry and Hazard/Shelter Data Planning

Status: Done

Completed:

- Added `data_pipeline/sources/source_candidates.json`.
- Registered candidate source families for shelter/facility data, tsunami/water-hazard data, and paper/secondary references.
- Documented official source vs secondary reference policy.
- Documented shelter point data vs hazard area/depth data separation.
- Added pytest coverage for source candidate registry structure and planning policy.

Scope:

- Planning and metadata only.
- Does not download large datasets, scrape websites, parse GIS, parse CityGML, modify Unity, or integrate with `Assets/Data`.

### Phase 3-02: Integrated Shelter/Hazard Pipeline Preparation and P4 Handoff

Status: Done

Completed:

- Added shelter source mapping template and mapping schema.
- Added local manual shelter ingestion fixture and adapter script.
- Added tsunami hazard schema, synthetic fixture, and validator.
- Added sample release package builder.
- Added P3 to P4 handoff docs and release checklist.
- Added pytest coverage for ingestion, hazard validation, release packaging, and existing P3 behavior.
- Updated `data_pipeline/run_pipeline.ps1` to run the integrated preparation flow.

Scope:

- Preparation and fixture milestone only.
- Does not download official datasets, scrape websites, parse large GIS files, parse CityGML, modify Unity, copy to `Assets/Data`, or modify PLATEAU imported files.

### Phase 4-A0: Workspace Setup, P3 Merge, and Documentation Baseline

Status: Done

Completed:

- Created `phase4-unity-real-data-integration`.
- Merged `origin/phase3-real-data-pipeline` through P3 commit `ca74ab0`.
- Confirmed expected P3 processed/release shelter and hazard sample outputs are present.
- Confirmed no forbidden Unity, PLATEAU, CityGML, `ProjectSettings`, or `Packages` paths changed in the merge diff.
- Added `docs/P4_REAL_DATA_INTEGRATION.md`.
- Recorded Unity batchmode and Python dependency environment warnings.

### Phase 4-A1: Real Shelter Loader and Source Mode Preparation

Status: Done

Completed:

- Copied the P3 release shelter sample to `Assets/Data/real_chuo_shelters_sample.json`.
- Kept `sourceMode = test` as the default in `Assets/Data/shelter_source_config.json`.
- Implemented `RealShelterDataLoader` for P3-shaped real shelter sample JSON.
- Implemented `ShelterDataSourceResolver` for `test` / `real_sample` source selection.
- Added missing-file fallback to test shelter data when configured.
- Rejected Unity runtime loading from `data_pipeline` paths.
- Added focused EditMode tests for source selection, real sample parsing, nullable fields, fallback behavior, and existing test-shelter compatibility.
- Preserved gameplay rules, Unity scenes, PLATEAU imports, and hazard gameplay effects.

Warning:

- EditMode batchmode launch still did not produce `test-results/editmode-results.xml`; manual Unity test follow-up remains required.

### Phase 4-B: Real Shelter Markers and Debug Hazard Fixture

Status: Done

Completed:

- Added real shelter gameplay mapping from A1 `real_sample` records into existing shelter-compatible data.
- Kept committed `sourceMode = test` as the default.
- Added runtime-only real shelter marker generation for `sourceMode = real_sample`.
- Generated markers use existing `BuildingShelter` and `ShelterEntranceTrigger` components.
- Added marker labels for real shelter metadata.
- Added deterministic debug layout fallback when `unityPosition` is absent.
- Copied the P3 hazard fixture to `Assets/Data/sample_tsunami_hazard_zones.json`.
- Added a hazard fixture loader and schematic debug visualizer toggled with `H` in Play Mode.
- Confirmed in code/tests that hazard debug shapes have no gameplay rule effect.
- Added focused EditMode tests for shelter mapping and hazard fixture visualization data.

Manual validation completed:

- Existing `sourceMode = test` shelter gameplay works.
- `real_sample` generates multiple real markers and supports entry/climb/result flow.
- Compact and detailed real shelter metadata display works.
- `H` hazard visualization appears without changing tsunami risk wall behavior.
- `sourceMode` was restored to `test`.

### Phase 4-BV: Automated Real Shelter and Hazard Debug Validation

Status: Done

Completed:

- Strengthened EditMode tests for committed `sourceMode = test`.
- Added EditMode coverage for `real_sample` marker-ready shelter mapping.
- Added EditMode coverage for deterministic fallback marker positions and metadata formatting.
- Added EditMode coverage for hazard fixture loading, schematic hazard debug layout, and no gameplay rule effect.
- Added Unity runtime hazard path guard rejecting `data_pipeline` paths.
- Added a minimal PlayMode smoke test for temporary real shelter marker generation and hazard debug visualization toggling.

Validation status:

- Run EditMode and PlayMode tests from Unity Editor Test Runner.
- EditMode Test Runner `Run All` passed.
- PlayMode Test Runner `Run All` passed.
- CLI batchmode wrapper remains a tooling warning because it may fail to produce `test-results/editmode-results.xml`.

### Phase 4-C: Real Data Debug Label Readability Polish

Status: Done

Completed:

- Changed real shelter labels to compact defaults: shelter name, facility type, capacity, and safe floor.
- Preserved detailed shelter metadata behind an `M` toggle in Play Mode.
- Reduced shelter label text size and raised labels to reduce overlap.
- Changed hazard labels to compact defaults: zone name, family, level, and depth.
- Reduced hazard label text size while preserving the existing `H` visualization toggle.
- Updated focused tests for compact labels and detailed metadata availability.

Validation status:

- `sourceMode` remains `test`.
- CLI EditMode wrapper still failed to produce `test-results/editmode-results.xml`.

Manual validation completed:

- Real shelter compact labels are readable.
- `M` detailed shelter metadata toggle works.
- `H` hazard visualization still toggles.
- Gameplay result rules are unchanged.
- `sourceMode` was restored to `test`.

### Phase 4 Final Closure

Status: Complete; merged into `master` after DeepSeek final review

Completed:

- P4-A0 branch/merge baseline.
- P4-A1 loader/sourceMode integration.
- P4-B real shelter markers and hazard debug visualization.
- P4-BV automated validation.
- Fallback layout stabilization.
- P4-C debug label readability polish.
- Unity Editor EditMode `Run All` passed.
- Unity Editor PlayMode `Run All` passed.
- Manual validation passed for default test gameplay, real sample markers, metadata display, entry/climb/result flow, `H` hazard visualization, hazard no-gameplay-effect behavior, and unchanged tsunami risk wall behavior.
- Default committed `sourceMode` remains `test`.

Future work:

- Fix CLI batchmode test-results XML reliability.
- Optional additional debug label polish.
- Optional real map alignment.
- Optional PLATEAU building matching.
- Optional richer hazard visualization.

## Milestone 4: Risk Zone Data

Status: Not started

Goal:
Move risk zone setup into data-backed test configuration.

Tasks:

- Define risk zone data fields.
- Load test risk zones from data.
- Validate warning messages and failure rules.
- Connect risk zone data to GameManager-facing runtime objects.

## Milestone 5: Result Review

Status: Not started

Goal:
Show a simple result and reflection panel.

Tasks:

- Show success / failure.
- Show selected shelter.
- Show elapsed time.
- Show failure reason.
- Prepare text useful for PBL presentation.

## Development Rule

Before each milestone:
1. Run a grill-me design check.
2. Update Markdown specs.
3. Generate a Codex prompt.
4. Implement only the current milestone.
5. Test in Unity.
6. Run DeepSeek review.
7. Commit and push.

---

## Current Testing Policy

Until the real street-placement system is implemented, first playable systems should be tested on an isolated debug platform.

The test platform should support:

- third-person movement testing
- sprint testing
- mouse orbit camera testing
- shelter entry testing
- climb simulation testing
- tsunami risk wall testing
- risk zone failure testing

Do not require the first playable loop to identify real streets from the current Buildings / LOD1 model.

## Phase 4 — Unity Integration of P3 Real Data Pipeline Outputs

Status: Complete after DeepSeek final review.

Completed:
- P4-A0 branch setup, P3 merge, workspace baseline
- P4-A1 real shelter loader and sourceMode
- P4-B real shelter markers and hazard debug visualization
- P4-BV automated validation
- fallback layout validation fix
- P4-C debug label readability polish
- Unity Editor EditMode / PlayMode validation
- manual real_sample and hazard visualization validation
- DeepSeek final review with no A-level blockers

Future non-blocking follow-up:
- Replace reflection-based ShelterEntranceTrigger setup with public setup API if needed
- Improve hazard debug material fallback robustness
- Clarify behavior when disabling existing test shelters in complex scenes
- Reduce fixed Assets/Data path assumptions in tests if project structure changes
- Fix CLI batchmode test-results XML issue

## Phase 5-B — Real Chuo Qualification + Routing Pipeline

Status: Complete after DeepSeek final review.

Completed:
- Official Chuo/Tokyo/GSI evacuation data ingestion
- Real Chuo shelter normalization
- PLATEAU building qualification and matching
- OSM walking-route sample generation
- Integrated route + qualification outputs
- QGIS QA for building matching and routes
- DeepSeek final review with no A-level blockers

Next:
- P5-C Unity read-only integration of qualified buildings, route lines, confidence, warnings, and decision feedback
