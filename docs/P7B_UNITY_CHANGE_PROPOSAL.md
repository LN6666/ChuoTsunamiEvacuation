# P7-B Unity Change Proposal

Date: 2026-05-22

Stage: P7-B Wave 1

Status: Proposal only. P7-B Wave 2 Unity changes require explicit approval before implementation.

## Approval Gate

No Unity file may be changed by P7-B Wave 1. P7-B Wave 2 must receive explicit approval before creating or editing any scene, script, imported asset, data file, package file, or project setting.

Approval must name the exact allowed paths and confirm whether Unity tests and benchmark records are required.

## Proposed Future Allowed Paths

If approved, Wave 2 should use the smallest practical allowlist. Candidate paths are:

| Path | Proposed Use | Notes |
|---|---|---|
| `Assets/Scenes/P7B_SmallAreaBenchmark.unity` | isolated benchmark scene | Must not be `Chuo_BaseMap.unity`. |
| `Assets/Scripts/Editor/P7B/` | optional editor-only benchmark helpers | Only if metrics cannot be collected manually or by existing tools. |
| `Assets/Tests/EditMode/P7B/` | focused editor/helper validation tests | Use only if Wave 2 adds editor helpers or scene validation. |
| `Assets/Tests/PlayMode/P7B/` | focused runtime or scene behavior tests | Use only if Wave 2 changes runtime behavior or benchmark scene behavior. |
| `docs/p7_benchmark_records/` | Markdown benchmark records | Prefer Markdown summaries over large raw profiler output. |

These paths are proposals, not permission.

## Possible Benchmark Scene

Possible scene name:

```text
Assets/Scenes/P7B_SmallAreaBenchmark.unity
```

Scene constraints:

- isolated from current gameplay scenes
- no dependency on `Chuo_BaseMap.unity`
- no broad Chuo import
- only the approved small path cluster
- no permanent gameplay rule wiring
- no player success/failure behavior changes
- no package or project setting edits

## Possible Scripts Or Editor Tools

Only after approval, a small editor helper may be considered for:

- capturing scene object counts
- recording loaded renderer/material/texture counts
- sampling simple Editor FPS or timing values
- exporting benchmark summaries to Markdown or CSV

Any helper should be editor-only unless a player-build benchmark specifically requires runtime collection. Runtime helpers need separate approval because they touch gameplay assembly boundaries and PlayMode behavior.

## Tests Required If Approved

If Wave 2 creates or modifies Unity files, run:

- Unity EditMode tests for editor utilities, scene validation, and path guard behavior
- Unity PlayMode tests if runtime components, scene behavior, or gameplay-facing objects change
- `powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1`
- protected path check with `git diff --name-only`
- benchmark Markdown validation if a record is created

Unity tests are intentionally not run for Wave 1 because this change is documentation-only.

## Must Not Be Touched

P7-B Wave 2 must not touch these paths unless a later prompt explicitly overrides this proposal:

- `Assets/Scenes/Chuo_BaseMap.unity`
- `Assets/PLATEAU/`
- `Assets/Data/`
- `ProjectSettings/`
- `Packages/`
- broad imported Chuo asset folders
- local raw PLATEAU data under `D:\PLATEAU_DATA\Chuo_2025_CityGML`

It also must not add dependencies, download data, create builds as tracked artifacts, or implement P8/P9 systems.

## Decision Needed Before Wave 2

Before implementation, the human developer should approve:

- benchmark area source filter or mesh-code slice
- target LOD and categories
- exact Unity paths allowed
- whether an editor helper is allowed
- whether an EXE benchmark is in scope for the stage
- where generated outputs may be written
- rollback conditions for scene/import removal
