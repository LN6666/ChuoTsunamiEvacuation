# Project Tasks

## Current Phase

The project has completed environment setup, PLATEAU SDK installation, local Chuo City Buildings / LOD1 import, and the first playable prototype.

The first playable prototype has been Unity-tested, DeepSeek V4 Pro max-thinking reviewed, committed, and pushed to GitHub.

Current next milestone:

Milestone 2: Rules and Dataization 1.0

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

Status: Next

Goal:
Move prototype rules and manually configured values into small, validated data/config structures while preserving the current first playable behavior.

Tasks:

- Define tsunami event config.
- Keep manual T warning start for debug.
- Ensure countdown starts only after tsunami warning.
- Add future random tsunami warning delay config.
- Define shelter config data.
- Define anti-camping config.
- Define result metrics.
- Add data loaders.
- Add editor validation support.

Acceptance Criteria:

- First playable behavior remains testable on the isolated debug platform.
- Tsunami timing, shelter rules, anti-camping settings, and result metrics are represented as data/config.
- Data loading validates required fields and logs clear errors.
- Editor validation can detect missing or invalid milestone data.

## Milestone 3: Chuo Data Integration

Status: Not started

Goal:
Connect the dataized gameplay loop to a small, controlled Chuo City shelter dataset without relying on inferred streets from Buildings / LOD1.

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
