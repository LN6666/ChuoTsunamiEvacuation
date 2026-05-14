# Technical Decisions

This document records important technical and design decisions.

---

## Decision 001: Use Unity as the main development platform

Date: 2026-05-12

Decision:
Use Unity as the main platform for the evacuation simulation game.

Reason:
Unity supports 3D interaction, player movement, triggers, UI, scene management, and gameplay logic. It is more suitable than a pure GIS tool for building an interactive serious game.

---

## Decision 002: Use PLATEAU CityGML as the 3D city model source

Date: 2026-05-12

Decision:
Use Project PLATEAU 3D city model data for Tokyo's Chuo City.

Reason:
PLATEAU provides official 3D urban model data, including buildings and related geographic datasets. It is suitable for creating a realistic urban base map.

---

## Decision 003: Use Buildings / LOD1 for the first base map

Date: 2026-05-12

Decision:
The first import uses only Buildings / bldg / LOD1.

Reason:
LOD1 is sufficient to show building massing and urban layout. It is lighter than LOD2 and more stable for the first Unity import.

Out of scope for first import:
- LOD2
- textures
- road models
- bridges
- water
- underground areas
- vegetation
- city furniture
- disaster risk layers

---

## Decision 004: Do not commit generated PLATEAU scene files to GitHub

Date: 2026-05-12

Decision:
The generated Chuo_BaseMap.unity scene file is kept locally and ignored by Git.

Reason:
The generated Unity scene is about 567MB. It is too large for normal GitHub usage and may cause repository bloat.

Policy:
- Store generated scene locally on the cloud desktop data disk.
- Commit only code, documentation, project settings, and small configuration files.
- Record import steps in docs/plateau_import_log_2026-05-12.md.

---

## Decision 005: Represent tsunami as a risk boundary, not fluid simulation

Date: 2026-05-12

Decision:
The tsunami is represented as a moving risk boundary or risk wall.

Reason:
Physically accurate fluid simulation is outside the project scope. The project focuses on evacuation decisions under shrinking safe space.

Implication:
The wall represents danger progression, not exact water dynamics.

---

## Decision 006: Use Codex as the main coding agent

Date: 2026-05-12

Decision:
Use Codex CLI as the main coding agent.

Reason:
Codex can read the Unity project, create C# scripts, modify files, and help fix compile errors.

Constraint:
Codex must follow AGENTS.md and must not modify unrelated files.

---

## Decision 007: Use DeepSeek V4 Pro API for code review

Date: 2026-05-12

Decision:
Use DeepSeek V4 Pro API as a code review tool through tools/deepseek_review.py.

Reason:
DeepSeek is suitable for long-context review of git diff, architecture, Unity lifecycle problems, and maintainability issues.

Constraint:
DeepSeek does not directly modify files. It only produces review reports.

---

## Decision 008: Use Markdown as the project coordination layer

Date: 2026-05-12

Decision:
Use Markdown documents to coordinate project planning, agent prompts, architecture, data schema, review workflow, and progress.

Reason:
Agent-based development requires explicit instructions, boundaries, and review criteria. Markdown provides a stable shared reference for the human developer, Codex, DeepSeek, and future presentation materials.

---

## Decision 009: Apply grill-me before implementing core systems

Date: 2026-05-12

Decision:
Before implementing core systems, run a grill-me design check.

Reason:
The project involves many ambiguous decisions, such as tsunami behavior, shelter validity, failure conditions, and player interaction. Grill-me helps expose hidden assumptions before code generation.

---

## Decision 010: First playable version should prioritize a vertical slice

Date: 2026-05-12

Decision:
The first coding milestone should implement a minimal playable evacuation loop.

Reason:
A small playable loop is more valuable than many disconnected systems.

First loop:
Player moves → tsunami risk approaches → player enters shelter → climb simulation → success or failure.

---

## Decision 011: Shelter entry is not immediate safety

Date: 2026-05-13

Decision:
Pressing E near a shelter starts the shelter entry / climb process, but it does not immediately guarantee safety.

The player becomes safe only after the climb simulation is completed.

Reason:
In a real tsunami evacuation situation, simply reaching a building entrance is not enough. If the tsunami reaches the player or the building entrance before the evacuee reaches a safe upper floor, the evacuation should be considered a failure.

Implication:
During Climbing state, failure can still happen.

Failure can be triggered by:
- tsunami risk reaching the player
- tsunami risk reaching the active shelter entrance
- countdown reaching zero before climb completion

---

## Decision 012: Future gameplay camera should be third-person

Date: 2026-05-13

Decision:
The project should move toward a third-person camera and movement system.

Reason:
The game is about urban evacuation behavior. A third-person camera makes it easier to understand the player's position, nearby buildings, risk boundaries, and shelter entrances.

Implication:
The first prototype may still use simple capsule objects, but the camera and movement system should be designed to support third-person gameplay.

The camera should:
- follow behind and above the player
- allow mouse-controlled view rotation
- keep the player visible
- avoid pure first-person camera behavior

---

## Decision 013: Tsunami countdown starts only after warning

Date: 2026-05-13

Decision:
The evacuation countdown should not start at the beginning of the game. It should start only after the tsunami warning is triggered.

Prototype:
The T key can manually trigger the tsunami warning for testing.

Future version:
The warning should occur after a random delay.

Reason:
In reality, the disaster situation is unexpected. The player should not begin the game already in a countdown state.

---

## Decision 014: Add future anti-camping rule

Date: 2026-05-13

Decision:
If the player waits inside a shelter entrance trigger before the tsunami warning, that shelter may be marked as camped and blocked for the current round.

Reason:
Without this rule, the player can exploit prior knowledge by waiting at a known safe building before the tsunami warning starts.

Implication:
The player must react to the disaster and search for a usable shelter, rather than simply waiting near a known shelter.

---

## Decision 015: Add future ordinary pedestrian phase

Date: 2026-05-13

Decision:
Future versions should include an ordinary pedestrian phase before the tsunami warning.

Examples:
- walking to a station
- going to an office
- going to a convenience store
- following a normal route

Reason:
This makes the tsunami warning feel unexpected and makes the evacuation decision more realistic.

---

## Decision 016: Use GTA / mobile-style third-person camera as the target camera model

Date: 2026-05-13

Decision:
The project should use a GTA-like or mobile-style third-person camera as the long-term camera model.

Reason:
The player needs to understand their position in the city, nearby shelter entrances, buildings, and approaching tsunami risk. A third-person orbit camera gives better spatial awareness than a first-person camera.

Design implications:

- Mouse orbit is the primary camera control.
- Q / E camera rotation is only a debug fallback.
- The player remains visible on screen.
- Movement is camera-relative.
- The camera can rotate around the player even when the player is not moving.

---

## Decision 017: Use isolated debug platform before real street placement

Date: 2026-05-13

Decision:
The first playable prototype should be tested on an isolated debug platform, not directly inside the imported PLATEAU building area.

Reason:
The current PLATEAU import uses Buildings / LOD1 only. It does not provide enough information to reliably distinguish streets, sidewalks, walkable ground, and building interiors.

Implication:
The imported PLATEAU city model is used as background and context during early gameplay testing.

The real street-based version should be implemented later, after road data, spawn point logic, walkable areas, and collision strategy are prepared.

---

## Decision 018: Separate visible tsunami wall from gameplay failure logic

Date: 2026-05-15

Decision:
The visible tsunami wall is visual feedback and backup trigger detection.

Gameplay failure is based on tsunami risk-front / flooded-side logic, not only collision with the finite visible wall.

Reason:
Unity testing showed that the player could walk around the finite visible wall and avoid failure even after the tsunami front had logically passed their position.

Implication:
- The moving wall still provides clear visual feedback.
- Existing trigger/contact failure remains as an additional safety mechanism.
- Player failure occurs when the moving risk front passes the player's projected position.
- Shelter-climbing failure occurs when the moving risk front passes the active shelter entrance before climb completion.
- This remains a simplified risk-boundary model, not a fluid simulation.
