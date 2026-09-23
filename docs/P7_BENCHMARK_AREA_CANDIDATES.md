# P7 Benchmark Area Candidates

Generated: 2026-05-22 22:05:15 +09:00

Inventory run ID: 20260522_220515

Branch: p7a-asset-inventory-lod-area

## Selection Position

No final benchmark area is selected by this report. The candidates below are path/name and file-metadata hints only, intended to narrow P7-B planning after human review.

## Criteria

| Criterion | Use |
|---|---|
| LOD3/LOD4 availability | Prefer candidates with explicit high-detail path/name evidence, then verify geometry before import. |
| Shelter/high-rise relevance | Prefer building-heavy areas that can support evacuation-building presentation needs. |
| Bridge/riverfront relevance | Prefer areas that show waterfront or bridge continuity for Chuo context. |
| Underground/road relevance | Prefer transportation or underground-related categories only when local assets provide evidence. |
| Asset size controllability | Prefer small, bounded folders or mesh-code slices before broad imports. |
| Risk of scene/import instability | Treat very large source sets or high file counts as risk until benchmarked. |
| Windows EXE benchmark suitability | Prefer candidates representative enough to measure but small enough to build and rerun. |

## Candidate Areas Or Categories

| Candidate | Files | Bytes | Categories | LOD evidence | Shelter/high-rise | Bridge/riverfront | Underground/road | Size control | Import risk | Windows EXE suitability |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Local PLATEAU source/udx/bldg | 68148 | 2.88 GB | Buildings | LOD2, LOD3 | High-rise/shelter context candidate | Not evident | Not evident | Low | High | Useful but needs smaller mesh-code or area slice |
| Local PLATEAU source/udx/brid | 754 | 306.19 MB | Bridges | Unknown by path/name | Indirect context only | Relevant | Not evident | Low | Medium | Useful but needs smaller mesh-code or area slice |
| Local PLATEAU source/udx/tran | 22 | 405.75 MB | Roads/transportation | Unknown by path/name | Indirect context only | Not evident | Relevant | Medium | Medium | Good candidate after small-area bounds are confirmed |
| Local PLATEAU source/udx/wtr | 3 | 41.88 MB | Water | Unknown by path/name | Indirect context only | Relevant | Not evident | High | Low | Good candidate after small-area bounds are confirmed |
| Local PLATEAU source/udx/ubld | 1 | 21.70 MB | Underground buildings | Unknown by path/name | Indirect context only | Not evident | Relevant | High | Medium | Good candidate after small-area bounds are confirmed |

## Recommendation

- Carry 2-5 of the candidates above into P7-B planning only after confirming exact area bounds.
- If all LOD evidence remains unknown, start P7-B with the smallest representative category or mesh-code slice instead of a broad import.
- Keep generated Unity scenes, imported PLATEAU assets, and build outputs local-only unless a later prompt explicitly approves a tracked artifact.
