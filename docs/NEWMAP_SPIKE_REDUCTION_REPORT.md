# NewMap Spike Reduction Report

Generated: 2026-05-27T04:00:17+09:00

Focus: startup/frame spike reduction. Memory is recorded only as informational in this task.

## Before

- Max frame: 20655.05 ms
- Average FPS: 46.2
- Stutter frames over 66 ms: 2

## Implemented Changes

- Player runtime scene-wide renderer/collider bounds scan is disabled.
- Runtime uses lightweight origin/support placement in player builds.
- Player-startup MeshCollider shutdown is disabled; the previous run traversed 44,115 scene MeshColliders.
- Normal `Debug.Log` stack traces are disabled in player builds.
- Official shelter anchors use one bounded exact-name traversal for eligible GML ids plus per-object renderer bounds.
- NPC humanoids are deferred until crowd failure is enabled.
- Old route validation remains preflight/report work, not player startup work.

## Retest

- Decision: `spike_remaining_but_documented`
- Average FPS: 72.72
- Max frame: 11273.06 ms
- Stutter frames over 66 ms: 2
- Player.log errors: 0
- Player.log warnings: 0
- Runtime bootstrap timing: bounds 0 ms, spawn/support 13 ms, systems 49 ms, targets 105 ms, configure 2 ms, total 169 ms

Attribution: the remaining 11.273 s max frame is not caused by NewMap runtime bootstrap, target activation, route validation, crowd creation, or MeshCollider shutdown. Those runtime paths measured 169 ms total. The remaining spike is attributed to Unity activation/render startup of the large Chuo_BaseMap PLATEAU scene before regular gameplay can be deferred.
