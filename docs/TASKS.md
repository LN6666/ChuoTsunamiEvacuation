# Project Tasks

## Current Phase

The project has completed environment setup and PLATEAU SDK installation.
Chuo City Buildings / LOD1 have been imported locally into Unity.

The next phase is to build the first playable evacuation loop.

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

Status: In progress

Tasks:

- Create AGENTS.md
- Create GAMEPLAY_SPEC.md
- Create TASKS.md
- Create ARCHITECTURE.md
- Create DATA_SCHEMA.md
- Create DECISIONS.md
- Create CODE_REVIEW_WORKFLOW.md

## Milestone 2: First Playable Core

Status: Not started

Goal:
Create the first playable evacuation loop.

Scripts to create:

- Assets/Scripts/Core/EvacuationGameManager.cs
- Assets/Scripts/Player/SimplePlayerController.cs
- Assets/Scripts/UI/GameUIManager.cs
- Assets/Scripts/Tsunami/TsunamiCountdownManager.cs
- Assets/Scripts/Tsunami/MovingTsunamiWall.cs
- Assets/Scripts/Tsunami/RiskZone.cs
- Assets/Scripts/Shelter/BuildingShelter.cs
- Assets/Scripts/Shelter/ShelterEntranceTrigger.cs
- Assets/Scripts/Shelter/ClimbSimulation.cs
- Assets/Scripts/Result/ResultPanelController.cs

Acceptance Criteria:

- Player can move in the scene.
- A countdown is visible.
- A tsunami risk wall can move.
- A test shelter can be entered by pressing E.
- Climb simulation can complete.
- Success and failure states are shown.
- Code compiles without Unity Console errors.

## Milestone 3: Shelter Data

Status: Not started

Goal:
Prepare a simple shelter data structure.

Tasks:

- Define shelter CSV / JSON fields.
- Create a small test shelter dataset.
- Add DataLoader.
- Connect shelter data to BuildingShelter components.

## Milestone 4: Risk Zone System

Status: Not started

Goal:
Make tsunami risk areas interactive.

Tasks:

- Define risk zone trigger logic.
- Add player failure detection.
- Add warning messages.
- Connect risk zones to GameManager.

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
