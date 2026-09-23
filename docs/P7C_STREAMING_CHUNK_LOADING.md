# P7-C Streaming And Chunk Loading

## Scope

P7-C is a benchmark sandbox foundation for chunk/loading, visual-quality review, and approximate performance telemetry.

It is not production Chuo streaming, not a full Chuo import, and not integration into `Chuo_BaseMap.unity`.

Allowed runtime scope:

- `Assets/P7Benchmark/`
- `Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity`
- `Assets/Scripts/P7Benchmark/`
- `Assets/Editor/P7Benchmark/`
- `Assets/Tests/*/P7Benchmark/`

Protected paths remain out of scope:

- `Assets/Scenes/Chuo_BaseMap.unity`
- production scenes outside `Assets/Scenes/P7Benchmark/`
- existing gameplay scripts outside `Assets/Scripts/P7Benchmark/`
- `Assets/PLATEAU/`
- `Assets/Data/`
- `ProjectSettings/`
- `Packages/`

## Confirmed Implementation Plan

The confirmed P7-C plan is:

1. Inspect imported candidate `53393690` as file metadata.
2. Build a `P7BenchmarkChunkRegistry` asset from metadata only.
3. Add a `P7BenchmarkChunkController` that can enable or disable logical placeholder roots.
4. Extend `P7BenchmarkMetricsRecorder` with active chunk count and imported candidate summary fields.
5. Update only the P7Benchmark skeleton scene with P7-C root objects and placeholder chunk groups.
6. Document visual quality, batching, memory, LOD, culling, collision, and P7-D profiling handoff risks.
7. Keep all claims limited to sandbox evidence.

## Implementation Summary

P7-C adds:

- `Assets/Scripts/P7Benchmark/P7BenchmarkChunkInfo.cs`
- `Assets/Scripts/P7Benchmark/P7BenchmarkChunkRegistry.cs`
- `Assets/Scripts/P7Benchmark/P7BenchmarkChunkController.cs`
- `Assets/Editor/P7Benchmark/P7CChunkRegistryBuilder.cs`
- `Assets/P7Benchmark/P7C_53393690_ChunkRegistry.asset`

The registry is metadata-driven. It does not convert CityGML, does not create production meshes, and does not depend on gameplay managers.

## Candidate Groups

`53393690` is represented as six logical groups:

| Group | Files | Bytes | Renderable Assets | P7-C Handling |
|---|---:|---:|---:|---|
| bldg | 5181 | 272392412 | 0 | placeholder chunk |
| brid | 43 | 16122211 | 0 | placeholder chunk |
| fld | 1 | 17335453 | 0 | placeholder chunk |
| frn | 616 | 269508019 | 0 | placeholder chunk |
| tran | 1 | 46558793 | 0 | placeholder chunk |
| veg | 1 | 12865355 | 0 | placeholder chunk |

Because the import contains `.gml` and `.jpg` files only, P7-C does not claim that these groups are renderable Unity mesh chunks.

## Scene Additions

Only `Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity` is updated.

Scene additions:

- `P7C_ChunkLoadingRoot`
- `P7C_ChunkController`
- `P7C_Metadata_53393690_raw_citygml_unconverted`
- six placeholder chunk group roots
- metadata-only placeholder volumes

The scene remains a benchmark sandbox. It has no `EvacuationGameManager` dependency and no gameplay success/failure effect.

## Tooling

Inspection:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/inspect_p7c_benchmark_import.ps1
```

Scene/registry build:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/build_p7c_benchmark_scene.ps1 -LaunchMode Gui
```

Preflight:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7c_preflight.ps1
```

## Boundary

P7-C must not implement P8 tsunami hazard, inundation, flood, light curtain, or risk-front systems.

P7-C must not implement P9 crowd, real spawn, indoor evacuation, congestion, indoor shelter gameplay, or failure systems.

P7-D remains the Windows EXE profiling and final P7 closeout stage.
