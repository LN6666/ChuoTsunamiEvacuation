# P7-D P8/P9 Handoff

Validation date: 2026-05-23.

## Baseline Recommendation

Use `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` as the user-approved practical high-detail baseline for P8/P9/P10.

The current imported level is accepted for continuation even though average LOD3 was not achieved, several categories are missing, and EXE profiling has not completed. Those items are known limitations and follow-ups, not blockers to starting P8/P9/P10 on this scene.

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

P8 should begin from `P7_HighDetail_Chuo` unless a future blocker appears. Do not implement tsunami fluid simulation. Do not claim official inundation/hazard correctness from this import.

If P8 needs water, terrain, disaster-risk polygons, or validated hazard placement, handle those gaps with data-layer, proxy, or rule-based approaches first, or schedule a focused follow-up import. The gaps do not invalidate the accepted baseline.

## P9 Guidance

P9 should also use `P7_HighDetail_Chuo` as the baseline. Where detailed entrances, underground spaces, interiors, or crowd-related geometry are missing or low-detail, use markers, rule-based nodes, proxy colliders, and representative interior templates.

Do not claim complete real-spawn, indoor evacuation, congestion, underground evacuation, or official route validity from geometry that is not present or verified.

## Legacy Fallback

`Chuo_BaseMap.unity` remains the legacy fallback policy, but this checkout does not currently contain or track that scene. If restored locally, it must remain untouched.
