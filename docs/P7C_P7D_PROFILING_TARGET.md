# P7-C P7-D Profiling Target

## Target Scene

P7-D target scene:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

P7-D must not profile the old test scene or only `Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity` as the final high-detail result.

## Final Profiling Requirement

Final P7-D profiling must include actual high-detail city assets. Placeholder roots, metadata labels, and the P7Benchmark skeleton are not enough for final profiling.

If `P7_HighDetail_Chuo.unity` is not fully populated, P7-D must first complete or validate PLATEAU SDK import before treating Windows EXE profiling as final.

## Metrics To Capture In P7-D

- Windows EXE average FPS
- Windows EXE 1 percent low FPS
- CPU main thread timing
- render thread timing
- GPU timing where available
- memory usage
- load and activation time
- draw calls and batches
- triangle and vertex counts
- texture, material, and mesh counts
- chunk/layer enable-disable behavior

## Baseline Decision

If profiling and compatibility checks pass, P7-D should declare `P7_HighDetail_Chuo.unity` as the baseline for P8/P9/P10. If profiling blocks, `Chuo_BaseMap.unity` remains a legacy fallback until the blocker is resolved.
