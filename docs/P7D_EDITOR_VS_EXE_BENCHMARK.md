# P7-D Editor vs EXE Benchmark

Validation date: 2026-05-23.

## Status

Editor vs EXE benchmark status: PREPARED, EXE DATA NOT COLLECTED.

The imported scene is renderable and heavy enough that Editor behavior should not be used as final performance evidence. Windows x64 profiling remains required before final baseline approval.

## Editor-Side Static Evidence

| Metric | Value |
|---|---:|
| Scene size | 22,554,882,711 bytes |
| MeshRenderer count | 117,728 |
| MeshFilter count | 117,728 |
| MeshCollider count | 117,728 |
| LODGroup count | 0 |
| PLATEAUCityObjectGroup count | 117,728 |

## Runtime Metrics

| Metric | Editor | Windows EXE |
|---|---|---|
| Average FPS | Not collected | Not collected |
| 1 percent low FPS | Not collected | Not collected |
| Loading time | Not collected | Not collected |
| Memory usage | Not collected | Not collected |
| Draw calls/batches | Not collected | Not collected |
| Build size | N/A | Not collected |

## Interpretation

The current evidence supports only a conditional baseline decision. EXE profiling must be run before P8/P9 systems increase scene load, renderer count, runtime logic, or data overlays.
