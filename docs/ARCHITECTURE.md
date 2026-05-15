# Architecture

## Overview

This project uses Unity as the main interaction engine and PLATEAU as the 3D city model source.

Current completed state:
- The first playable prototype is complete and Unity-tested.
- Milestone 2-01 - Data-Driven Rules Integration is complete and Unity-tested.
- Milestone 2-02 - Scenarioized Gameplay Rules 1.0 is complete.
- Milestone 2-03 - Unity Test Automation Foundation is complete.
- Milestone 2-04 - Multi-Shelter Decision Gameplay 1.0 has been implemented for the debug platform.
- The prototype runs on an isolated debug platform while PLATEAU geometry is used as background/context.
- Tsunami warning currently starts manually with T for debug.
- Countdown and tsunami movement begin only after the warning event.
- Tsunami timing, multi-shelter test rules, scenario presets, anti-camping settings, and result metrics are data-driven.

Current milestone:
- Milestone 2: Rules and Dataization 1.0
- Remaining focus: editor validation support and future build-safe data loading.

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
- keep visible wall contact as backup trigger detection
- run tsunami risk-front / flooded-side failure checks during active movement
- report player failure when the risk front passes the player
- report shelter failure when the risk front passes the active shelter entrance during climb
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
- expose shelter rank, official/candidate status, entry delay, climb time, crowding delay, and failure reason
- carry real-data-ready optional fields such as source type, facility type, safe floor, capacity, and future PLATEAU/facility identifiers
- accept matching test shelter data without overwriting Inspector values when data is missing

### ShelterEntranceTrigger

Responsibility:
- detect player near entrance
- show shelter decision information and interaction prompt
- handle E key interaction
- call BuildingShelter validation

### ClimbSimulation

Responsibility:
- simulate going to a safe floor
- wait for total entry + climb + crowding time
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
- show explainable ResultMetrics fields for prototype review
- support PBL presentation and reflection

## Data Loading System

### GameConfigLoader

Responsibility:
- load tsunami_event_config.json
- load anti_camping_config.json
- provide safe defaults and warnings when configs are missing or invalid

Current limitation:
- uses Application.dataPath + "/Data/..." for the Editor-stage prototype
- should later migrate to StreamingAssets or another build-safe path

### ShelterDataLoader

Responsibility:
- load test_shelters.json
- provide lookup by shelterId
- provide all loaded test shelters for debug-platform generation
- apply scenario shelter overrides in memory only
- preserve scene/Inspector shelter values when shelterId is missing or unknown

### ScenarioPresetLoader

Responsibility:
- load scenario_presets.json
- resolve the active scenario
- apply tsunami, anti-camping, and shelter overrides in memory
- fall back to default behavior for missing or unknown scenarios
- avoid modifying base JSON files at runtime

### ResultMetrics

Responsibility:
- store explainable result data such as selected shelter, timing, risk arrival, delays, and camping flags
- keep result data separate from UI text formatting
- include active scenario identifiers for decision review

Milestone 2 data loading focuses on:

- tsunami event config
- multi-shelter test config data
- future random warning timing config
- anti-camping config
- scenario presets
- result metric definitions

Milestone 2-04 keeps the data on the debug platform and does not consume P3 real-data pipeline outputs yet.

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
- P2-04 multi-shelter gameplay uses test shelters only; future P4 work should integrate P3 processed real shelter data.

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
