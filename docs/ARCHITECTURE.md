# Architecture

## Overview

This project uses Unity as the main interaction engine and PLATEAU as the 3D city model source.

Current completed state:
- The first playable prototype is complete and Unity-tested.
- The prototype runs on an isolated debug platform while PLATEAU geometry is used as background/context.
- Tsunami warning currently starts manually with T for debug.
- Countdown and tsunami movement begin only after the warning event.

Current next milestone:
- Milestone 2: Rules and Dataization 1.0
- Focus: tsunami event config, shelter config data, anti-camping config, result metrics, data loaders, and editor validation support.

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
- keep PreEvent, evacuation, climbing, success, and failure state transitions explicit

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
- remain idle before the tsunami warning starts

### MovingTsunamiWall

Responsibility:
- move risk boundary from start point to end point
- provide visual risk movement
- optionally trigger risk activation events
- remain idle before the tsunami warning starts

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

Milestone 2 data loading should focus on:

- tsunami event config
- shelter config data
- future random warning timing config
- anti-camping config
- result metric definitions

The first playable prototype may continue to use manually placed test objects while these config structures are introduced.

### Editor Validation

Responsibility:
- check required config files and fields
- report invalid values before Play mode testing
- detect missing shelter IDs, invalid timing values, and missing debug-start config

It should not:
- modify PLATEAU imported scene files
- modify raw PLATEAU data
- infer streets or walkable areas from Buildings / LOD1 geometry

## PLATEAU Integration

PLATEAU data is used as a 3D city base map.

Current policy:
- Buildings / bldg / LOD1 are imported locally.
- Generated large scene files are not committed to GitHub.
- Raw PLATEAU data is not committed to GitHub.
- PLATEAU-specific logic should remain isolated.
- Current gameplay-loop testing uses an isolated platform away from PLATEAU building geometry.

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
