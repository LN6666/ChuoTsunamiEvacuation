# P10-C-Pre User Performance Playtest Guide

Branch: `p10c-pre-performance-gate-optimization`

Temporary build path, if created:

```text
D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe
```

This is not the final release build and not a release package.

## What To Observe

- startup and first playable time
- Low profile FPS and stutter at 1080p
- tsunami start responsiveness
- green frame activation
- light curtain activation
- crowd/congestion scenario responsiveness
- ResultPanel opening
- weather/night visibility and performance
- Player.log warnings/errors
- working set, private memory, disk activity, and hard-fault symptoms

## Blocking Issues

- persistent freeze or unresponsive player
- Low profile far below playable 30 FPS target
- green frames or light curtain freeze the game
- memory grows continuously during a short run
- disk paging causes repeated stalls
- Player.log spams errors or exceptions

## Quick-Fix Report Template

Quality profile / scenario / timestamp / action taken / FPS or stutter symptom / working set / private memory / Player.log warning count / Player.log error count / whether green frames, light curtain, crowd, UI, weather, and ResultPanel were active.
