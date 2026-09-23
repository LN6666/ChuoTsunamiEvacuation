# P7-B Benchmark Harness Design

Date: 2026-05-22

Stage: P7-B Wave 1

Status: Design only. No Unity scene, script, asset, import, package, or project setting change is approved by this document.

## Purpose

P7-B needs a small-area high-detail benchmark harness before any broader Chuo import or optimization work. The harness should answer whether a bounded PLATEAU path cluster can run acceptably in the Unity Editor and, when available, a Windows x64 EXE without mutating the existing gameplay baseline.

P7 has exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D.

## Non-Mutation Rule For Wave 1

This Wave 1 design does not modify:

- `Assets/Scenes/`
- `Assets/Scripts/`
- `Assets/Data/`
- `Assets/PLATEAU/`
- `ProjectSettings/`
- `Packages/`
- imported PLATEAU assets

The existing `Assets/Scenes/Chuo_BaseMap.unity` scene is not a benchmark target and must remain untouched unless a later prompt explicitly approves a change.

## Future Harness Shape

The future P7-B Wave 2 harness should use an isolated benchmark scene, not `Chuo_BaseMap.unity`. A possible scene name is:

```text
Assets/Scenes/P7B_SmallAreaBenchmark.unity
```

The benchmark scene should contain only:

- one approved small path cluster selected from P7-A/P7-B evidence
- the minimum camera path or fixed camera points needed for repeatable metrics
- temporary benchmark-only lighting and camera setup if required
- benchmark-only marker objects that do not affect gameplay systems
- optional measurement helpers approved in the Wave 2 change plan

It must not become a broad Chuo staging scene or an alternate gameplay scene.

## Area Scope

The benchmark area should be a small path cluster, not a ward-wide import. It should include only the smallest representative set of categories required to test the P7-B question:

- dense building visual detail
- road or transportation context if available for the selected slice
- bridge or waterfront context when the selected slice includes it
- underground category evidence only if P7-A/P7-B source records show a controlled local candidate

The selected cluster must have a written source filter, approximate bounds or mesh-code slice, included categories, target LODs, excluded categories, and rollback notes before import.

## Measurement Modes

### Editor Metrics

Editor measurement is required for Wave 2 if Unity content is created. Record:

- Unity version
- benchmark scene name
- selected area identifier
- target LOD and category set
- Game view resolution
- quality/render pipeline settings as observed
- average FPS, 1% low FPS, and minimum FPS when available
- RAM and texture memory or VRAM when available
- draw calls, batches, SetPass calls, triangles, and vertices when available
- loaded object count when available
- loading time to usable scene state
- camera path or fixed viewpoint list
- bottleneck classification and notes

### Optional Windows EXE Metrics

Windows x64 EXE metrics are optional for P7-B Wave 2 if no build exists yet, but required before P7-D closeout. If an EXE benchmark is available, record:

- build date/time
- development build status
- build size
- target resolution and window mode
- machine specs and GPU driver if available
- average FPS, 1% low FPS, and minimum FPS when available
- RAM and VRAM or texture memory when available
- loading time
- difference from Editor benchmark behavior

## Performance Thresholds

Preferred target:

- average FPS at or above 60
- 1% low FPS at or above 45
- no severe stutter on the benchmark route
- stable RAM/VRAM over repeated measurement

Minimum prototype threshold:

- average FPS at or above 30
- 1% low FPS at or above 24
- no crash
- no unbounded memory growth
- no multi-second stall during normal benchmark movement

If the benchmark misses the minimum threshold, P7-B should reduce area size, LOD coverage, material/texture cost, or collision scope before considering broader import work.

## Rollback Criteria

Rollback the benchmark change if any of these occur:

- `Chuo_BaseMap.unity` changes without explicit approval
- protected paths outside the Wave 2 allowlist change
- the small cluster expands into a broad Chuo import
- benchmark scene load becomes impractically slow
- Editor or EXE performance misses the minimum threshold after obvious scope reduction
- generated artifacts are too large for Git or include raw PLATEAU data
- Unity tests or P7 preflight fail and cannot be fixed within the approved scope

## Outputs

Future Wave 2 should produce:

- one isolated benchmark scene or a documented reason a scene was not created
- one Markdown benchmark record under `docs/p7_benchmark_records/`
- a protected-path check summary
- Unity EditMode and PlayMode test results when scene or runtime behavior changes
- an updated decision entry with accept/reduce/rollback result

Wave 1 produces only design, proposal, test-plan, rollback-plan, task, backlog, decision-log, protocol, automation-plan, and review-prompt documentation.
