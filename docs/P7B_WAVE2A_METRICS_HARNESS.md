# P7-B Wave 2-A Metrics Harness

Date: 2026-05-22

Stage: P7-B Wave 2-A

Status: Implemented pending Unity validation.

## Runtime Components

Wave 2-A adds two runtime scripts under `Assets/Scripts/P7Benchmark/`:

- `P7BenchmarkMarker.cs`
- `P7BenchmarkMetricsRecorder.cs`

Both scripts are benchmark-only and have no references to gameplay managers, shelters, tsunami systems, NPC systems, routes, hazards, `Assets/Data`, or PLATEAU imports.

## Recorder Metrics

`P7BenchmarkMetricsRecorder` supports:

- frame-time sample collection
- elapsed sampled time
- sample count
- average FPS
- approximate 1 percent low FPS
- average frame time in milliseconds
- exportable summary string

The recorder can run from `Update` after `BeginRecording()` or accept explicit samples through `RecordFrameTime(float deltaSeconds)`. Invalid samples such as zero, negative, NaN, and infinity are ignored.

## Approximate 1 Percent Low

The 1 percent low value is approximate. It sorts valid frame-time samples from slowest to fastest, averages the slowest 1 percent of samples, and converts that average slow-frame time to FPS. For empty samples it returns `0`. For small sample sets it safely uses at least one valid sample.

## Scope Limitations

These metrics are prototype benchmark metrics. They are not a replacement for Unity Profiler, Profile Analyzer, Memory Profiler, RenderDoc, or Windows EXE profiling.

The harness does not collect draw calls, batches, SetPass calls, GPU timing, memory, VRAM, triangle counts, texture memory, or load-time profiler data. Those remain later benchmark/profiling work.

Wave 2-A metrics do not make pass/fail decisions for real LOD3 imports. They only provide a small reusable recorder for the future isolated benchmark scene.

