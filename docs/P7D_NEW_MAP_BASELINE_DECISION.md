# P7-D New Map Baseline Decision

Decision: PASS - USER-APPROVED PRACTICAL HIGH-DETAIL BASELINE.

## Meaning

The user reviewed the lower-than-original-target LOD/category coverage and explicitly accepts `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` as the practical local high-detail baseline for P8/P9/P10 continuation on this cloud PC.

This decision does not claim average LOD3 and does not claim a full-category PLATEAU import. The LOD/category gaps are known limitations for future phases, not blockers to starting P8/P9/P10.

## Evidence

| Criterion | Status |
|---|---|
| Actual manual import exists | Pass: 117,728 renderable PLATEAU city-object groups detected |
| Average LOD3 target | Known limitation: actual evidence is LOD0-LOD2, with no verified LOD3 |
| Buildings | Accepted limitation: renderable, LOD0-LOD2 |
| Roads / transport | Accepted limitation: renderable, LOD0-LOD1 |
| Bridges | Accepted limitation: renderable, LOD1, 13 objects |
| Underground | Accepted limitation: renderable, LOD1, 1 object |
| Water / terrain / disaster risk / land use / urban planning / vegetation / city furniture | Known missing layers in converted scene evidence |
| P2-P6 compatibility | Accepted for handoff: source-compatible, runtime scene smoke remains a follow-up |
| Windows EXE profiling | Follow-up on accepted baseline: prepared, not run |
| GitHub persistence | Known limitation: scene is 22.55 GB and local-only unless a later archive/release strategy stores it outside normal Git |

## P8/P9 Start Decision

P8 should start from `P7_HighDetail_Chuo` as the accepted practical baseline unless a future blocker appears. Missing or low-detail hazard-related layers should be handled through data-layer, proxy, or rule-based approaches where needed.

P9 should also use `P7_HighDetail_Chuo` as the baseline, with markers, rule-based nodes, proxy colliders, and representative interior templates where detailed entrances, underground spaces, interiors, or crowd-related geometry are missing.

Do not claim official route safety, complete underground/interior coverage, average LOD3, or full-category PLATEAU completion from this P7-D evidence.

## Fallback

`Chuo_BaseMap.unity` remains the legacy fallback conceptually, but it is not present/tracked in this checkout. It must remain untouched if restored locally.
