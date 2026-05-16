# Gameplay Specification

## Project Concept

This project is a 3D tsunami evacuation serious game set in Tokyo's Chuo City.

The player acts as a pedestrian after an earthquake. The goal is to find a usable evacuation building before the tsunami risk boundary reaches the area.

## Core Gameplay Loop

1. The player starts in Chuo City.
2. An earthquake / tsunami warning is shown.
3. A tsunami risk boundary begins moving from the waterfront side.
4. The player searches for a usable shelter building.
5. The player approaches a shelter entrance.
6. The player presses E to enter.
7. A simplified climb simulation starts.
8. The player succeeds if they reach a safe floor before the risk arrives.
9. The player fails if they remain in the risk zone or cannot enter a valid shelter.
10. A result panel explains the outcome.
11. The run result can be exported locally for review.

## First Playable Version

The first playable version should include:

- player movement
- simple camera control
- countdown timer
- moving tsunami risk wall
- risk zone trigger
- shelter building data
- shelter entrance trigger
- climb simulation
- success / failure state
- simple result UI

## Out of Scope for First Version

The first version will not include:

- real fluid simulation
- full tsunami hydrodynamics
- full crowd simulation
- indoor building navigation
- complex pathfinding
- detailed UI polish
- all Chuo buildings as interactable shelters
- automatic structural safety judgment for every building

## Tsunami Representation

The tsunami is represented as a risk boundary, not as a physically accurate water simulation.

The purpose is to show that safe space decreases over time and that the player must make evacuation decisions under time pressure.

## Shelter Interaction

Shelters are simplified as interactable building entrances.

Each shelter may have:

- building name
- shelter rank
- official shelter flag
- can enter flag
- post-earthquake status
- climb time
- failure reason

## Multi-Shelter Decision Prototype

Milestone 2-04 upgrades the debug-platform prototype from one shelter to several test shelter choices.

The generated debug platform includes:

- a near official shelter
- a farther fast shelter
- a crowded candidate shelter
- a slow official shelter
- an intentionally blocked shelter

The goal is to make the player compare distance, rank, official status, enterability, climb time, crowding delay, and scenario effects before pressing E.

The current fixture still uses test shelters only. It does not import real Chuo facility data, match PLATEAU buildings, infer roads, or detect real entrances.

## Evaluation and Decision Feedback

Milestone 2-05 adds lightweight evaluation support for the debug-platform prototype.

When the game reaches success or failure:

- ResultMetrics records the selected shelter, scenario, timing, camping flags, and outcome reason.
- ResultPanel shows short next-step advice.
- ResultExportService writes CSV and JSON run logs under run_logs/.

Advice remains simple and prototype-oriented. It distinguishes blocked shelters, anti-camping blocks, late risk failure, crowding delay, successful fast choices, and generic fallback cases.

The export is for local evaluation only. It is not a dashboard or analytics system.

## Success Conditions

The player succeeds when:

- they enter a usable shelter
- the shelter is available after the earthquake
- the climb simulation finishes before failure is triggered

## Failure Conditions

The player fails when:

- the tsunami risk boundary reaches the player
- the player enters an invalid shelter
- the timer reaches zero
- the selected building is not usable
