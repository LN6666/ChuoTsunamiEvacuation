# P7-D Windows EXE Profiling Report

Windows EXE profiling status: PREPARED, NOT RUN.

## Current Evidence

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` now contains renderable PLATEAU content, so EXE profiling is meaningful enough to attempt.

Profiling was not completed in this Codex pass because the project does not currently have a committed P7-D Windows build/profiling automation path, and generating large build/profiler outputs must not be committed blindly.

## Available Scene Metrics

| Metric | Value |
|---|---:|
| Scene size | 22,554,882,711 bytes |
| MeshRenderer count | 117,728 |
| MeshFilter count | 117,728 |
| MeshCollider count | 117,728 |
| PLATEAUCityObjectGroup count | 117,728 |
| LODGroup count | 0 |
| Average LOD3 achieved | No |

## EXE Metrics

| Metric | Result |
|---|---|
| Build target | Pending Windows x64 |
| Build type | Pending |
| Average FPS | Not collected |
| Approximate 1 percent low FPS | Not collected |
| Loading time | Not collected |
| RAM/memory notes | Not collected |
| Texture/mesh memory notes | Not collected |
| Draw calls/batches | Not collected |
| Build size | Not collected |
| Bottleneck observations | Pending |
| Acceptable for P8/P9 baseline | Not proven |

## Manual Profiling Checklist

1. Create a Windows x64 development build that opens `P7_HighDetail_Chuo`.
2. Store the build outside Git, for example under a local `Builds/` folder or cloud-drive staging folder.
3. Record load time from process launch to stable camera view.
4. Record average FPS and approximate 1 percent low FPS over a fixed camera path or stationary view.
5. Record memory usage from Task Manager or Unity Profiler.
6. Record draw calls/batches and mesh/texture memory from Unity Profiler if available.
7. Archive only final release/build artifacts to cloud drive; do not commit build output or profiler binaries.
