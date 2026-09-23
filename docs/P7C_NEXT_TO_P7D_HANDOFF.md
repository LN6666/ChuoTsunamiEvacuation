# P7-C To P7-D Handoff

## P7-C Completion Target

P7-C prepares a benchmark-only chunk/loading foundation for candidate `53393690`.

Before P7-D starts, confirm:

- P7-C preflight passes.
- Unity GUI EditMode tests pass.
- Unity GUI PlayMode tests pass.
- DeepSeek has no A-level blockers.
- `Chuo_BaseMap.unity` is untouched.
- `ProjectSettings` and `Packages` are clean.
- `Assets/PLATEAU` and `Assets/Data` are untouched.

## P7-D Scope

P7-D is the Windows EXE profiling and final P7 closeout stage.

P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

## P7-D Profiling Work

P7-D should run a Windows x64 EXE benchmark and record:

- build target and build configuration
- scene used
- machine summary
- resolution and quality settings
- average FPS
- approximate 1 percent low FPS
- Unity Profiler CPU main/render thread evidence
- GPU timing where available
- memory usage
- texture/material/mesh counts
- draw calls and batches
- active chunk state
- load and activation timing

## P7-D Decision Questions

P7-D should decide:

- whether `53393690` remains feasible as a benchmark candidate
- whether CityGML conversion is required before further visual claims
- whether production streaming should continue, narrow scope, or stay deferred
- whether ProjectSettings or package changes need a separate approval plan
- whether P7 closeout can proceed without production integration

## Deferred Follow-Ups

- CityGML renderable mesh conversion remains separate work.
- Full production streaming remains deferred.
- Unity Profiler remains required for authoritative diagnosis.
- Windows EXE evidence remains required before final P7 performance conclusions.
