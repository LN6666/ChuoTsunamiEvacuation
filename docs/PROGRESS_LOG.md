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
