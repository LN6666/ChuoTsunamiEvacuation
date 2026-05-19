# Project Tasks

## Current Phase

The project has completed environment setup, PLATEAU SDK installation, local Chuo City Buildings / LOD1 import, the first playable prototype, Phase 3 real data pipeline preparation, and Phase 4 Unity integration of the P3 real shelter/hazard sample.

The first playable prototype has been Unity-tested, DeepSeek V4 Pro max-thinking reviewed, committed, and pushed to GitHub.

Current branch focus:

Phase 5 qualification, routing, PLATEAU matching, and Unity map-decision integration planning is starting on `phase5-qualification-routing-plateau`.

## Phase 5: Qualification, Routing, PLATEAU Matching, and Unity Integration

Status: P5-B3 environment and source readiness complete.

Planned tasks:

- P5-A0: workspace setup, documentation baseline, and open-source reference candidate registry. Done.
- P5-A1: official evidence source review and open-source reference decisions. Done.
- P5-A2: evacuation building qualification rulebook foundation. Done.
- P5-B1: controlled sample qualification/matching/routing pipeline using the P5-A2 schema. Done.
- P5-B2: real Chuo ingestion readiness, dependency/environment plan, CRS/QGIS QA plan, and P5-B3 execution plan. Done.
- P5-B3: project-local Python GIS environment helper and source provenance/fixture readiness. Done.
- P5-B4: run or verify local P5 environment with user approval, complete source provenance/license review, and create a small controlled real-source fixture. Next.
- P5-B: PLATEAU qualification/matching and GIS routing pipeline.
- P5-C: Unity integration for qualified buildings, routes, confidence, warnings, and decision feedback.
- P5 final review: validation summary, DeepSeek review, scope boundary confirmation, and merge decision.

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
