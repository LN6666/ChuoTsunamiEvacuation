# First Playable Specification

## Purpose

This document defines the first playable version of the ChuoTsunamiEvacuation project.

The goal is to create a minimal but complete evacuation gameplay loop on top of the imported Chuo City PLATEAU building scene.

## First Playable Goal

The player should be able to:

1. Start inside the Chuo City 3D scene.
2. Move around the city.
3. See a tsunami countdown.
4. See a moving tsunami risk wall.
5. Approach a test shelter entrance.
6. Press E to enter the shelter.
7. Wait during a simplified climb simulation.
8. Reach a success or failure result.

## Default Design Decisions

### Player Spawn Area

The first version uses the Harumi / Kachidoki / Tsukishima area as the intended player spawn area.

Reason:
This waterfront area is more relevant to a tsunami evacuation scenario.

### Tsunami Risk Boundary

The tsunami is represented by a simple straight risk wall.

The wall moves from the Tokyo Bay / waterfront side toward the inland side.

The wall does not represent physically accurate water movement.
It represents a moving risk boundary.

### Failure Condition

If the player touches or enters the active tsunami risk wall / risk zone, the game immediately triggers failure.

No grace period is used in the first version.

### Shelter Buildings

The first version uses manually placed test shelters.

It does not automatically classify all PLATEAU buildings.
It does not use the full official shelter dataset yet.

Initial shelter count:

- 3 test shelter objects

### Shelter Interaction

The player enters a shelter by approaching an entrance trigger and pressing E.

The interaction should show:

- shelter name
- shelter rank
- whether the shelter can be entered
- prompt text

### Climb Simulation

The project does not use indoor models in the first version.

After the player enters a shelter, the game starts a simplified climb simulation.

During climb simulation:

- player movement can be disabled
- UI shows climbing progress
- after a short wait, success is triggered if failure has not happened

Initial climb time:

- 10 to 20 seconds

### UI Requirements

The first version should include simple UI for:

- tsunami countdown
- warning / instruction text
- shelter name
- interaction prompt
- climb progress
- success result
- failure result

Visual polish is not required in the first version.

### Real Data Usage

The first version does not use official shelter CSV / JSON yet.

Manual test objects are used first.

Official data integration will be handled in a later milestone.

## Out of Scope

The first playable version will not include:

- real tsunami fluid simulation
- indoor navigation
- official full shelter database integration
- automatic shelter classification from all buildings
- crowd simulation
- complex route guidance
- minimap
- polished UI art
- multiplayer
- advanced animation

## Acceptance Criteria

The first playable version is considered successful if:

1. Unity compiles without red Console errors.
2. Player movement works.
3. Countdown is visible.
4. Tsunami risk wall moves.
5. At least one shelter entrance trigger works.
6. Pressing E starts climb simulation.
7. Success result can be triggered.
8. Failure result can be triggered by risk wall / risk zone.
9. All scripts are committed to GitHub after review.

## Implementation Policy

Codex should create small, modular scripts.

Codex should not modify unrelated files.

The first implementation should not depend on PLATEAU-specific APIs.

The system should work with manually placed GameObjects in the Unity scene.
