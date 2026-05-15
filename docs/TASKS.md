# Project Tasks

## Current Phase

The project has completed environment setup, PLATEAU SDK installation, local Chuo City Buildings / LOD1 import, the first playable prototype, and Phase 3-00 real data pipeline scaffold.

The first playable prototype has been Unity-tested, DeepSeek V4 Pro max-thinking reviewed, committed, and pushed to GitHub.

Current branch focus:

Phase 3: Real Chuo Data Pipeline

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

Tasks:

- Define shelter CSV / JSON fields.
- Create a small test shelter dataset.
- Add DataLoader.
- Connect shelter data to BuildingShelter components.

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
