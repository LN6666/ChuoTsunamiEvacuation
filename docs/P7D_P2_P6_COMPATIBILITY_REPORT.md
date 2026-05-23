# P7-D P2-P6 Compatibility Report

Validation date: 2026-05-23.

## Status

Compatibility status: CONDITIONAL PASS, runtime scene smoke still pending.

The new high-detail scene contains renderable PLATEAU objects, so compatibility can be assessed beyond the old shell state. P2-P6 source systems remain scene-independent enough to adapt, but P7-D did not wire production gameplay into the new map and did not change success/failure rules.

## P2

| Area | Status | Evidence |
|---|---|---|
| Player movement | Compatible in source, pending map placement smoke | `Assets/Scripts/Player/SimplePlayerController.cs` is not bound to `Chuo_BaseMap`. |
| Camera | Compatible in source, pending new-scene gameplay camera smoke | First playable builder creates cameras independently of the P7 scene. |
| Shelter interaction | Compatible in source, pending representative high-detail target placement | `ShelterEntranceTrigger` and `BuildingShelter` do not require `Chuo_BaseMap`. |
| Result panel / success-failure flow | Compatible, unchanged | P7-D did not modify result or game-state rules. |

## P3

P3 data pipeline outputs remain compatible with runtime data assumptions because they are static JSON/CSV style data and not bound to the old base map scene. Coordinate transform and placement validation remain a known follow-up before official geospatial claims.

## P4

Real shelter loading and marker placement can adapt to the new map after coordinate-transform and representative marker placement validation. P7-D does not claim real shelters are already correctly placed on the imported scene.

## P5

Qualified shelters, route metadata, and high-rise/humanitarian candidate display logic remain opt-in and fail-safe:

- `Assets/Data/shelter_source_config.json` remains `sourceMode = test`.
- `enableHumanitarianCandidates = false`.
- `enableLifeFirstCandidateSelection = false`.
- No unsafe official-route claims were added.

## P6

Navigation guidance and NPC prototype code target objects rather than `Chuo_BaseMap` directly. They can be staged against the new scene, but P7-D did not perform a full runtime smoke with high-detail scene targets and did not add P9 crowd, real spawn, indoor evacuation, congestion, or failure systems.

## Pending Runtime Smoke

- Player spawn height and collision on the imported map.
- Gameplay camera framing in the imported map.
- Representative shelter trigger placement against imported buildings.
- Result panel flow after a staged shelter interaction.
- P5 real-data loaders with high-detail marker placement.
- P6 guidance/NPC staging against imported target objects.
