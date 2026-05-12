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
