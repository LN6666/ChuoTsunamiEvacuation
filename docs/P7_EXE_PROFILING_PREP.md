# P7 EXE Profiling Prep

Date: 2026-05-22

Stage: P7-A

Status: Profiling readiness documentation only. No Windows build is created by this file.

## Why Windows EXE Profiling Matters

Unity Editor performance is useful for iteration, but the player experience depends on Windows x64 EXE behavior. Editor overhead, attached profilers, domain reloads, scene view rendering, and editor-only systems can distort FPS, memory, and loading measurements.

P7 must compare Editor and EXE data before making final high-detail city asset decisions.

## What To Record

Each Windows EXE profiling record should include:

- benchmark ID, timestamp, branch, and commit
- machine specs
- build path or build identifier
- build size
- development build status
- resolution and fullscreen/windowed mode
- scene/setup and asset scope
- LOD level and graphics settings
- average FPS and 1% low FPS
- RAM and VRAM or texture memory
- draw calls, batches, and triangles
- loading time
- bottlenecks
- decision and notes

## Target Metrics

Preferred target:

- average FPS at or above 60
- 1% low FPS at or above 45
- stable memory under repeated route/load checks
- no severe loading or streaming stalls

Minimum prototype threshold:

- average FPS at or above 30
- 1% low FPS at or above 24
- no crash
- no unbounded memory growth
- no multi-second stall during normal movement

## Editor vs EXE Comparison

Compare Editor and EXE records by:

- using the same scene/setup where possible
- using the same camera route or fixed camera points
- recording the same asset scope and LOD level
- noting profiler attachment status
- separating Editor-only bottlenecks from player-build bottlenecks
- preferring EXE evidence for P7-D closeout decisions

If Editor and EXE results conflict, keep the decision conservative until the cause is understood.

## Stage Expectations

P7-B:

- EXE benchmark is optional if a suitable build exists.
- Editor benchmark remains acceptable for early small-area screening.

P7-C:

- EXE benchmark is recommended after streaming, LOD, or optimization work that could affect runtime behavior.

P7-D:

- Windows x64 EXE profiling is mandatory before final P7 closeout.
- Final P7 decisions should cite Markdown benchmark records.

## Deferred Work

P7-A does not create build automation, benchmark scenes, Unity scripts, package changes, or project setting changes. Those require later-stage approval and a confirmed Markdown plan.
