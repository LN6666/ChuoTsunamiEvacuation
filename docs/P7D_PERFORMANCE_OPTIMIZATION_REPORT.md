# P7-D Performance Optimization Report

## Status

Optimization status: BLOCKED ON MANUAL IMPORT.

The high-detail scene does not yet contain actual renderable PLATEAU assets. P7-D cannot optimize an empty/shell scene and cannot claim performance success.

## Methodology

P7-D follows measure first, optimize second:

1. Populate the high-detail scene with actual PLATEAU assets.
2. Measure Editor behavior only as preliminary data.
3. Build and profile a Windows EXE.
4. Identify CPU, GPU, memory, loading, rendering, culling, and collision bottlenecks.
5. Optimize only after bottlenecks are measured.

## Required Workflows

- Unity Profiler: CPU main thread, render thread, memory, loading spikes.
- Memory Profiler: texture memory, mesh memory, material count, scene object count.
- Frame Debugger: draw calls, batches, shader/material state changes, shadow passes.
- Windows EXE profiling: runtime FPS, 1 percent low FPS, loading time, RAM, build size.

## Risks To Measure After Import

- draw calls and batches from many unique materials
- triangle and vertex count from high-detail meshes
- texture count and texture memory pressure
- material explosion from per-surface textures
- mesh count and submesh count
- static batching memory tradeoff
- SRP Batcher compatibility
- GPU instancing only for repeated identical meshes
- dynamic batching not suitable as the primary high-detail city strategy
- frustum culling behavior on chunk roots
- occlusion culling bake feasibility
- collision simplification needs
- loading time and memory peak during scene activation

## Chunk / Scene Partition Strategy

Layer roots in `P7_HighDetail_Chuo.unity` should remain separable by category and later by spatial chunk. P7Benchmark chunk enable/disable logic remains metadata-safe and can be adapted only after actual imported roots exist.

## Collision Strategy

High-detail geometry must not become complex MeshCollider content by default. Future production integration should use simplified colliders only where gameplay requires them.

## Current Conclusion

No optimization success is claimed. P7-D is blocked until actual assets are loaded and measured.
