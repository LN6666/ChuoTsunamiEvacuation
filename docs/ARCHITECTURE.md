# Architecture

## Overview

This project uses Unity as the main interaction engine and PLATEAU as the 3D city model source.

The architecture is divided into several simple systems:

- Core Game Flow
- Player System
- Tsunami Risk System
- Shelter System
- UI System
- Result System
- Data Loading System
- Review / Tooling System

## Core Game Flow

### EvacuationGameManager

Responsibility:
- control game state
- start and stop gameplay
- handle success and failure
- coordinate timer, shelter, risk, and result systems

It should not:
- directly control player movement
- directly parse CSV / JSON
- directly manage visual UI details

## Player System

### SimplePlayerController

Responsibility:
- WASD movement
- mouse look or simple camera direction
- optional sprint
- CharacterController-based movement

It should not:
- decide game success or failure
- directly manage shelter data
- directly manage tsunami logic

## Tsunami Risk System

### TsunamiCountdownManager

Responsibility:
- manage remaining time
- notify GameManager when time is over
- expose remaining time to UI

### MovingTsunamiWall

Responsibility:
- move risk boundary from start point to end point
- provide visual risk movement
- optionally trigger risk activation events

### RiskZone

Responsibility:
- detect player entering risk area
- notify GameManager
- represent local danger areas

## Shelter System

### BuildingShelter

Responsibility:
- store shelter properties
- validate whether shelter can be used
- expose shelter rank, climb time, and failure reason

### ShelterEntranceTrigger

Responsibility:
- detect player near entrance
- show interaction prompt
- handle E key interaction
- call BuildingShelter validation

### ClimbSimulation

Responsibility:
- simulate going to a safe floor
- wait for climb time
- notify GameManager when completed

## UI System

### GameUIManager

Responsibility:
- show timer
- show warning messages
- show interaction prompt
- show climb progress

It should not:
- decide game state
- parse shelter data
- directly move the player

## Result System

### ResultPanelController

Responsibility:
- show success / failure
- show selected shelter
- show elapsed time
- show reason for failure
- support PBL presentation and reflection

## Data Loading System

### DataLoader

Responsibility:
- read CSV / JSON / GeoJSON data
- convert data into runtime structures
- avoid hardcoded data inside gameplay scripts

The first version may use manually placed test shelters before full data loading is implemented.

## PLATEAU Integration

PLATEAU data is used as a 3D city base map.

Current policy:
- Buildings / bldg / LOD1 are imported locally.
- Generated large scene files are not committed to GitHub.
- Raw PLATEAU data is not committed to GitHub.
- PLATEAU-specific logic should remain isolated.

## Agent Workflow

### Codex

Codex creates and modifies code based on explicit prompts.

### DeepSeek V4 Pro

DeepSeek reviews git diff and produces review reports.

### Human Role

The human developer:
- confirms requirements
- operates Unity
- tests gameplay
- approves changes
- commits stable milestones

## Design Principles

- Keep modules small.
- Avoid unnecessary inheritance.
- Prefer composition over large manager classes.
- Use serialized fields for Inspector configuration.
- Add null checks for scene references.
- Avoid heavy runtime allocations.
- Avoid overengineering in the first playable version.
- Make each module independently testable in Unity.
