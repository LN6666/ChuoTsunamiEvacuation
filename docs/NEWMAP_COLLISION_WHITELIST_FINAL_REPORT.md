# NewMap Collision Whitelist Final Report

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

## Final Blocking Policy

The player should be blocked only by:

- ground/support surface
- tight building footprint proxies
- NPC body/soft-blocking
- 3.5km circular map boundary

Everything else is nonblocking or trigger-only:

- shelter/route/direct-line visuals
- green frames
- labels
- markers
- hazard/light curtain visuals before active hazard logic
- debug/test objects
- old rectangular air walls

## Building Precision Layer

The whitelist now treats building blockers as tightened runtime footprint proxies rather than broad renderer/root bounds. Oversized cluster/root bounds are skipped. This keeps building blocking while reducing invisible walls in nearby roads and open walking areas.

## Status

Pending final temporary player validation and Player.log parsing for this hotfix.
