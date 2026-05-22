# P7 Benchmark Protocol

Date: 2026-05-22

Status: Protocol only. No benchmark import or Unity build is created in P7-0.

## Purpose

P7-B and later stages must benchmark high-detail city asset decisions before broad/full Chuo import. Benchmarks must be repeatable, recorded in Markdown, and separated between Unity Editor and Windows x64 EXE measurements.

## Benchmark Area Selection

Select small areas that represent P7 risk:

- Dense building block.
- Waterfront evacuation context.
- Bridge or road structure.
- Underground/station-adjacent candidate if source data supports it.
- A fallback LOD2-only area for performance comparison.

Each area record must include:

- Area name.
- Approximate bounds or source folder/filter.
- Included PLATEAU categories.
- Candidate LOD levels.
- Why the area is representative.
- Rollback criteria.

## Benchmark Variants

At minimum, compare:

- LOD2 baseline.
- LOD3 average target.
- LOD4 selected key-area target if available.
- Collision disabled vs simplified collision if interaction requires collision.
- Original material/texture setup vs reduced material/texture setup if a safe reduction exists.

## Metrics

Record these metrics for both Editor and Windows x64 EXE where available:

| Metric | Requirement |
|---|---|
| Average FPS | Preferred target: 60 FPS. |
| 1% low FPS | Minimum acceptable lower-bound stability indicator. |
| Minimum FPS | Record if available. |
| RAM | Record process/system memory where available. |
| VRAM / texture memory | Record if available from profiler, GPU tool, or Unity stats. |
| Draw calls / batches | Use Frame Debugger, Stats, or profiler evidence. |
| SetPass calls | Record if available. |
| Triangles / vertices | Record representative camera values. |
| Loaded object count | Record if tooling exposes it. |
| Loading time | Time to enter usable scene/state. |
| Build size | Required for Windows x64 EXE benchmark. |
| Bottlenecks | CPU, GPU, memory, loading, shader/material, texture, culling, or unknown. |

## Editor Benchmark

Editor measurements are useful for iteration risk but do not replace player-build measurements.

Editor record must include:

- Unity version.
- Scene or temporary benchmark setup.
- Game view resolution.
- Quality/render pipeline settings as observed.
- Whether profiler was attached.
- Whether deep profiling was disabled.
- Camera route or fixed camera points.
- Warm-up duration and sample duration.

## Windows x64 EXE Benchmark

Windows x64 EXE benchmark is required before P7-D closeout and should be used earlier when asset decisions could affect build runtime performance.

Windows record must include:

- Build date/time.
- Development build yes/no.
- Target resolution and fullscreen/windowed mode.
- Machine specs.
- GPU driver if available.
- Average FPS and 1% low FPS.
- RAM and build size.
- Loading time.
- Notes on whether Frame Debugger or Profiler was attached.

## Pass / Fail Thresholds

Preferred:

- Average FPS at or above 60.
- 1% low FPS at or above 45.
- No severe streaming stutter.
- RAM/VRAM stable over repeated route.

Minimum acceptable prototype threshold:

- Average FPS at or above 30.
- 1% low FPS at or above 24.
- No crash, no unbounded memory growth, no multi-second loading stall during normal movement.

Fail or rollback if:

- Average FPS is below 30 on target Windows x64 machine.
- Memory grows continuously during repeated load/unload.
- Build or scene load becomes impractically slow.
- Visual quality gains do not justify performance cost.
- Full Chuo import requires protected path or dependency changes not approved by the stage plan.

## Required Markdown Record

Use `docs/P7_PERFORMANCE_METRICS_TEMPLATE.md` for each benchmark run. Attach or reference screenshots only if they are small and approved for tracking. Large captures and profiler files should remain local unless explicitly approved.
