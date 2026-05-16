---

## 2026-05-17 | P4-A1 Real Shelter Sample Loader Implemented

### Completed

Implemented the P4-A1 data-layer path for `sourceMode = test / real_sample` while keeping `test` as the default.

Copied the validated P3 release shelter sample to `Assets/Data/real_chuo_shelters_sample.json` for Unity-readable loading. Runtime loading now rejects `data_pipeline` paths and uses the copied `Assets/Data` sample only.

Added `RealShelterDataLoader` for P3-shaped shelter JSON and `ShelterDataSourceResolver` for source-mode selection and missing-file fallback. Existing `test_shelters.json` loading remains unchanged.

Added focused EditMode tests for default source mode, test loading, P3-shaped real sample parsing, copied asset loading, nullable field handling, missing-file fallback, and rejecting `data_pipeline` runtime paths.

### Smoke Checks

- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode` failed to produce `test-results/editmode-results.xml`, matching the P4-A0 batchmode environment warning.
- PlayMode was not run because EditMode launch did not complete.

### Next Step

P4-B should generate real shelter markers from the loaded real sample, display real shelter metadata, validate shelter entry/result flow, and add debug-only hazard fixture visualization without replacing the tsunami risk wall or gameplay failure rules.

---

## 2026-05-17 | P4-A0 Workspace Baseline Completed

### Completed

Created `phase4-unity-real-data-integration` and merged `origin/phase3-real-data-pipeline` through commit `ca74ab0`.

Confirmed P3 `data_pipeline/` and processed/release sample outputs are present, with no `Assets`, `ProjectSettings`, `Packages`, `Chuo_BaseMap`, PLATEAU, or CityGML changes in the merge diff.

Added the P4 real-data integration baseline document.

### Smoke Checks

- Unity EditMode command launched Unity `6000.4.6f1` but did not produce `test-results/editmode-results.xml`; PlayMode was skipped for P4-A0 due to the same batchmode environment risk.
- `data_pipeline/run_pipeline.ps1` failed at step 1 because `jsonschema` is missing in the active Python environment.
- `python -m pytest --version` failed because `pytest` is not installed.

### Next Step

P4-A1 should implement the real shelter loader/sourceMode path while keeping `test` as the default and adding focused EditMode tests.

---

## 2026-05-16 | Milestone 2-05 Evaluation and Export Hooks Implemented

### Completed

Implemented Milestone 2-05 - Evaluation, Decision Feedback & Real-Data Integration Hooks 1.0.

Added run logging and export:

- Added ResultExportService for one-result-per-run export when gameplay reaches success or failure.
- Export writes CSV and JSON under run_logs/.
- run_logs/ is ignored by Git.
- Export records include runId, timestamp, scenario, outcome, selected shelter, shelter timing, countdown, risk timing, camping flags, and advice.

Improved decision feedback:

- ResultMetrics now exposes centralized outcome reason and next-step advice generation.
- ResultPanel continues to use the readable prototype/debug layout.
- Advice now covers blocked shelters, anti-camping blocks, late risk failure, crowding delay, and successful fast choices.

Added P4 real-data integration hooks:

- Added shelter_source_config.json with sourceMode defaulting to test.
- Added ShelterSourceConfigLoader.
- Added real_chuo_shelters_sample.example.json as an inactive example stub.
- P2-05 still uses test_shelters.json and does not read P3 data_pipeline outputs.

Updated tests:

- Added EditMode coverage for export payloads, CSV/JSON generation, missing optional fields, and advice strings.
- Added EditMode coverage for shelter source mode defaults and fallback behavior.
- Added PlayMode smoke coverage for ResultMetrics export record creation.

### Scope

This milestone remains a debug-platform prototype. It does not import real Chuo facility data, read P3 processed output, convert coordinates, match PLATEAU buildings, create a dashboard, or add NPC/crowd simulation.

---

## 2026-05-15 | Milestone 2-04 Multi-Shelter Decision Gameplay Implemented

### Completed

Implemented Milestone 2-04 - Multi-Shelter Decision Gameplay 1.0 for the debug platform prototype.

Added multi-shelter decision data:

- Expanded Assets/Data/test_shelters.json from a single normal shelter plus blocked shelter into five test records.
- Added Near Official Shelter, Far Fast Shelter, Crowded Candidate Shelter, Slow Safe Shelter, and Blocked Test Shelter.
- Extended test shelter records with real-data-ready optional fields such as sourceType, facilityType, layoutPosition, address, latitude, longitude, coordinateSystem, plateauBuildingId, safeFloor, capacity, source metadata, and notes.

Updated generated debug setup:

- FirstPlayableSceneBuilder now generates multiple shelter markers and entrance triggers from test_shelters.json.
- Shelter placement uses each record's layoutPosition on the isolated debug platform.
- PLATEAU geometry remains background/context only.

Updated scenarios:

- default uses the base multi-shelter fixture.
- normal_success keeps clear successful shelter options.
- random_warning starts the warning quickly while multiple shelters remain available.
- blocked_shelter blocks the nearest obvious shelter while leaving alternatives usable.
- anti_camping blocks only the shelter camped before warning.
- late_failure makes the slow shelter risky while preserving faster alternatives.

Updated result review:

- ResultPanel advice now reflects selected shelter decisions, blocked shelter attempts, camping blocks, and late risk failure.

Updated tests:

- Added EditMode checks for multiple shelters, unique IDs, blocked/enterable/official/crowded coverage, layout positions, scenario references, and late_failure tradeoff data.
- Added an EditMode smoke test for FirstPlayableSceneBuilder multi-shelter generation.

### Scope

This milestone remains a P2 debug-platform gameplay fixture.

It does not import real Chuo facility data, read from P3 data_pipeline outputs, match PLATEAU buildings, infer real roads, or create real entrance navigation.

---

## 2026-05-16 | Phase 3-02 Integrated Shelter/Hazard Pipeline Preparation Completed

### Completed

Added an integrated Phase 3 preparation milestone covering:

- manual shelter source mapping and local fixture ingestion
- tsunami hazard schema, synthetic fixture, and validator
- P3 sample release package builder
- P3 to P4 handoff docs and release checklist
- pytest coverage for ingestion, hazard validation, release packaging, and existing P3 behavior

### Scope Boundary

Phase 3-02 remains preparation-only. It does not download official datasets, scrape websites, parse large GIS files, parse CityGML, integrate with Unity, copy outputs to `Assets/Data`, or modify PLATEAU imported files.

---

## 2026-05-15 | Phase 3-01 Official Source Registry and Hazard Planning Completed

### Completed

Added a planning-only source candidate registry for future official Chuo/Tokyo data ingestion.

Covered source families:

- shelter / evacuation facility data
- tsunami and water-hazard area/depth data
- paper or secondary references for manual review only

Added pytest coverage for:

- registry JSON validity
- required candidate metadata fields
- allowed source families
- scraping policy
- secondary-reference official-status restrictions
- planning policy flags that block download, scraping, CityGML parsing, and Unity integration

### Scope Boundary

Phase 3-01 does not download datasets, scrape websites, parse GIS files, parse CityGML, modify Unity, integrate with `Assets/Data`, or modify PLATEAU imported files.

---

## 2026-05-15 | Phase 3-00 Real Data Pipeline Scaffold Completed

### Completed

Added the Phase 3 standalone real shelter data pipeline scaffold.

Created:

- data_pipeline source manifest
- real shelter JSON Schema
- synthetic sample shelter CSV
- exporter for Unity-ready sample JSON/CSV
- validator with JSON Schema and sanity checks
- pytest coverage for schema, export, validation, IDs, Unity interface fields, and coordinate ranges
- PowerShell one-command runner
- Phase 3 pipeline, schema, and P2/P3/P4 interface documentation

### Verified

`.\data_pipeline\run_pipeline.ps1` passes:

- export step
- validation step
- pytest suite

### Scope Boundary

Phase 3-00 does not modify Unity gameplay, Unity scenes, PLATEAU imported files, or existing `Assets/Data` gameplay JSON.

PLATEAU building matching, real source ingestion, geocoding, and Unity integration remain future Phase 3/4 work.

---

## 2026-05-15 | Milestone 2-02 Scenarioized Gameplay Rules Completed

### Completed

Milestone 2-02 - Scenarioized Gameplay Rules 1.0 was implemented, Unity-tested, reviewed, and cleaned up.

Added scenario data and loading:

- Assets/Data/scenario_presets.json
- default scenario
- normal_success scenario
- random_warning scenario
- blocked_shelter scenario
- anti_camping scenario
- late_failure scenario
- ScenarioPresetLoader

Formalized gameplay rules:

- Random warning min/max validation.
- Soft-lock prevention when manualStartEnabled=false and randomStartEnabled=false.
- Anti-camping behavior and round reset behavior.
- Scenario overrides applied in memory without rewriting base JSON config files.

Improved result explanation:

- ResultPanel evacuation review text now groups outcome, shelter, timing, risk, camping, scenario, and next-step advice.

Added test support:

- Scenario-related EditMode tests.
- Runtime assembly asmdefs needed by EditMode and PlayMode tests.

### Verified Behavior

Manual Unity scenario tests passed for:

- default
- random_warning
- blocked_shelter
- anti_camping
- late_failure

Automated tests:

- EditMode tests passed.
- PlayMode tests passed.

### Review

DeepSeek review found no A-level blockers.

Follow-up cleanup removed the UTF-8 BOM from scenario_presets.json.

---

## 2026-05-15 | Milestone 2-03 Test Automation Foundation Added

### Completed

Added a lightweight Unity test foundation for Milestone 2-03.

Added EditMode tests for:

- tsunami event config loading and safe defaults
- invalid and partial tsunami config sanitization
- test shelter data loading
- unknown shelterId preservation behavior
- anti-camping config defaults

Added PlayMode smoke tests for:

- minimal EvacuationGameManager startup without PLATEAU scene dependencies
- PreEvent startup state
- manual start method reaching Playing state
- missing tsunami config fallback helper in Play Mode

Added tooling and docs:

- tools/run_unity_tests.ps1
- docs/UNITY_TESTING_WORKFLOW.md

### Scope

This milestone is testing/tooling only. It does not automate full gameplay QA yet.

---

## 2026-05-15 | Milestone 2-01 Completed

### Completed

Milestone 2-01 - Data-Driven Rules Integration was implemented, Unity-tested, stabilized, and reviewed.

Added data/config files:

- Assets/Data/tsunami_event_config.json
- Assets/Data/test_shelters.json
- Assets/Data/anti_camping_config.json

Added runtime/data scripts:

- Assets/Scripts/Data/GameConfigLoader.cs
- Assets/Scripts/Data/ShelterDataLoader.cs
- Assets/Scripts/Result/ResultMetrics.cs

### Verified Behavior

- Manual T tsunami start works.
- Countdown starts only after tsunami warning.
- Random warning works when enabled.
- test_shelter_001 canEnter=false blocks entry and shows failureReason.
- climbTimeSeconds and crowdingDelaySeconds affect stair-climbing duration.
- Anti-camping detection and blocking work when enabled.
- Anti-camping is disabled by default.
- Missing tsunami config falls back to safe defaults and logs a warning.
- Missing or unknown shelterId preserves in-scene shelter values and logs a warning.
- ResultPanel is readable after the overflow fix.
- Walking around the finite visible tsunami wall now triggers failure when the tsunami risk front passes the player.
- Shelter climbing fails if the tsunami risk front passes the active shelter entrance.
- MarkSceneDirty Play Mode error was fixed.

### Review

Second DeepSeek V4 Pro review found no A-level blocking issues.

Milestone 2-01 is ready to commit after documenting B/C review items.

---

## 2026-05-14 | Planning Updated for Milestone 2

### Current Completed State

The first playable prototype is complete, Unity-tested, DeepSeek V4 Pro max-thinking reviewed, and pushed to GitHub.

Completed first playable features:

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
- DeepSeek max review workflow

### Next Milestone

Milestone 2: Rules and Dataization 1.0

Focus:

- Tsunami event config
- Shelter config data
- Countdown starts only after tsunami warning
- Manual T start remains for debug
- Future random tsunami warning config
- Anti-camping config
- Result metrics
- Data loaders
- Editor validation support

---

## 2026-05-14 | First Playable Prototype Verified

### Completed

The first playable evacuation prototype was implemented, reviewed, tested, and pushed to GitHub.

### Verified Features

- Third-person player movement works.
- WASD / arrow-key movement works.
- Shift sprint works.
- Mouse-based third-person camera control works.
- Camera-relative movement works.
- Shelter entrance interaction works with E.
- Climb simulation works.
- Success result can be triggered after climb completion.
- Tsunami test can be started with T.
- Tsunami risk can trigger failure.
- If the tsunami reaches the active shelter entrance during climb, failure is triggered.
- The generated test setup runs on an isolated debug platform instead of inside PLATEAU building geometry.

### Review

DeepSeek V4 Pro max-thinking review completed.

Review result:

Safe to commit.

### GitHub

The first playable prototype code has been committed and pushed to GitHub.

### Current Status

The project has moved from environment setup and PLATEAU import into a working first playable vertical slice.

### Next Phase

Next development phase is Milestone 2: Rules and Dataization 1.0.

The milestone should focus on:

- tsunami event config
- shelter config data
- countdown starts only after tsunami warning
- manual T start remains for debug
- future random tsunami warning config
- anti-camping config
- result metrics
- data loaders
- editor validation support
