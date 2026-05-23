# P7-D New Map Baseline Decision

Decision: CONDITIONAL PASS.

## Meaning

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is accepted as the intended local baseline candidate for early P8/P9/P10 continuation on this cloud PC, but it is not a fully approved high-detail or production-ready baseline.

## Evidence

| Criterion | Status |
|---|---|
| Actual manual import exists | Pass: 117,728 renderable PLATEAU city-object groups detected |
| Average LOD3 target | Fail: actual evidence is LOD0-LOD2, with no verified LOD3 |
| Buildings | Conditional: renderable, LOD0-LOD2 |
| Roads / transport | Conditional: renderable, LOD0-LOD1 |
| Bridges | Conditional: renderable, LOD1, 13 objects |
| Underground | Conditional: renderable, LOD1, 1 object |
| Water / terrain / disaster risk / land use / urban planning / vegetation / city furniture | Missing from converted scene evidence |
| P2-P6 compatibility | Conditional: source-compatible, runtime scene smoke pending |
| Windows EXE profiling | Prepared, not run |
| GitHub persistence | Conditional: scene is 22.55 GB and local-only unless a later archive/release strategy stores it outside normal Git |

## P8/P9 Start Decision

P8 can start on `P7_HighDetail_Chuo` only for limited, clearly marked work that does not require full LOD3 coverage, water/terrain/risk layers, or final performance proof.

P9 should not start crowd, indoor evacuation, congestion, underground evacuation, or official route claims from this map evidence alone.

If P8 requires final hazard placement over terrain/water/disaster-risk data, this scene is blocked until those categories are imported or another data source is accepted.

## Fallback

`Chuo_BaseMap.unity` remains the legacy fallback conceptually, but it is not present/tracked in this checkout. It must remain untouched if restored locally.
