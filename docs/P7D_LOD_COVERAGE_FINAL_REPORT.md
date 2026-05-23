# P7-D LOD Coverage Final Report

Validation date: 2026-05-23.

## Status

LOD coverage status: CONDITIONAL PASS WITH MATERIAL LIMITATIONS.

Average LOD3 achieved: FALSE.

The original P7-D target was an average LOD3 high-detail Chuo baseline. The actual manual PLATEAU SDK import did not meet that target. Scene inspection of `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` detects only LOD0-LOD2 city-object evidence and no verified LOD3 objects.

## Scene Evidence

- Scene size: 22,554,882,711 bytes.
- PLATEAUCityObjectGroup count: 117,728.
- MeshRenderer count: 117,728.
- MeshFilter count: 117,728.
- MeshCollider count: 117,728.
- LODGroup count: 0.
- Converted Unity output roots under `Assets/P7HighDetail/`, `Assets/P7HighDetail/PLATEAU/`, `Assets/P7HighDetail/Imported/`, and `Assets/PLATEAU/`: not present.
- The import evidence is embedded in the high-detail scene, not in isolated committed asset folders.

## Category Coverage

| Category | Target LOD | Actual detected LOD | Object count | Renderable evidence | Status |
|---|---:|---:|---:|---|---|
| Buildings | LOD3 | LOD0-LOD2 | 103,190 | MeshRenderer/MeshFilter/PLATEAU metadata | Partial, below target |
| Roads / transport | LOD3 | LOD0-LOD1 | 14,524 | MeshRenderer/MeshFilter/PLATEAU metadata | Partial, below target |
| Bridges | LOD3 | LOD1 | 13 | MeshRenderer/MeshFilter/PLATEAU metadata | Partial, below target |
| Underground | LOD3 if available | LOD1 | 1 | MeshRenderer/MeshFilter/PLATEAU metadata | Partial, below target |
| City furniture | LOD2/LOD3 | None | 0 | None | Missing |
| Water | LOD1 | None | 0 | None | Missing |
| Vegetation | LOD3 if available | None | 0 | None | Missing |
| Relief / terrain | Import | None | 0 | None | Missing |
| Disaster risk | Import | None | 0 | None | Missing |
| Land use | Import | None | 0 | None | Missing |
| Urban planning decision | LOD1 | None | 0 | None | Missing |

## LOD Counts

| LOD | Count |
|---:|---:|
| LOD0 | 37,022 |
| LOD1 | 51,560 |
| LOD2 | 29,146 |
| LOD3 | 0 |

## Impact On P8/P9/P10

P8 can use this scene only as a conditional local map baseline for early hazard-placement experiments that do not depend on LOD3 detail, water/terrain/risk layers, or complete category coverage.

P9 should not assume bridge, road, underground, crowd, indoor, or route realism from this import. The road/bridge/underground evidence is renderable but low-detail and incomplete.

P10 packaging must not assume this 22.55 GB scene is reproducible from GitHub alone. The local scene and any final build output must be archived to cloud drive before the VM is deleted.
