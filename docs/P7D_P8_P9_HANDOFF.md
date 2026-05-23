# P7-D P8/P9 Handoff

Validation date: 2026-05-23.

## Baseline Recommendation

Use `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` as a conditional local baseline only.

It is useful for early visual/context work because it contains renderable PLATEAU geometry. It is not a final P8/P9/P10 production baseline because average LOD3 was not achieved, many categories are missing, and EXE profiling has not completed.

## Reliable

- Renderable buildings exist.
- Renderable roads/transport objects exist.
- A small number of renderable bridges exist.
- One renderable underground object exists.
- P2-P6 source systems are not hard-bound to `Chuo_BaseMap`.
- Gameplay success/failure rules were not changed.

## Incomplete

- No verified LOD3 objects.
- No converted water, relief/terrain, disaster risk, land-use, urban-planning, vegetation, or city-furniture scene evidence.
- No P2-P6 runtime smoke test in the populated map.
- No Windows EXE profiling result.
- No proof that the 22.55 GB scene can be committed to normal GitHub safely.

## P8 Guidance

P8 may begin only if work is scoped as conditional map-baseline work. Do not implement tsunami fluid simulation. Do not claim official inundation/hazard correctness from this import.

If P8 needs water, terrain, disaster-risk polygons, or validated hazard placement, resolve those category gaps first.

## P9 Guidance

Do not implement P9 crowd, real spawn, indoor evacuation, congestion, underground evacuation, or failure systems from this P7-D result. The road/bridge/underground evidence is too limited for those claims.

## Legacy Fallback

`Chuo_BaseMap.unity` remains the legacy fallback policy, but this checkout does not currently contain or track that scene. If restored locally, it must remain untouched.
