# P7-D Performance Summary

## Current Verdict

Performance verdict: CONDITIONAL PASS, not production-approved.

The scene has enough renderable PLATEAU content for profiling preparation, but not enough measured runtime evidence for final P8/P9/P10 approval.

## Measured Static Evidence

- Scene: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- Scene size: 22,554,882,711 bytes.
- MeshRenderer count: 117,728.
- MeshFilter count: 117,728.
- MeshCollider count: 117,728.
- LODGroup count: 0.
- Actual LOD range: LOD0-LOD2.

## Missing Runtime Evidence

- Average FPS.
- Approximate 1 percent low FPS.
- Loading time.
- Runtime memory.
- Texture/mesh memory.
- Draw calls/batches.
- Build size.

## Decision Impact

P8/P9 may proceed only under the conditional baseline limits in `docs/P7D_NEW_MAP_BASELINE_DECISION.md`. EXE profiling should be the first follow-up before adding costly gameplay systems.
