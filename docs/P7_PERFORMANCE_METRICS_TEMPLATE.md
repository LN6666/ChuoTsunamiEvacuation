# P7 Performance Metrics Template

Copy this template for P7-B, P7-C, and P7-D benchmark records.

P7-A also provides `tools/p7/new_p7_benchmark_record.ps1` for a smaller timestamped skeleton and `tools/p7/validate_p7_performance_log.ps1` for required-field checks. Required fields are defined in `docs/P7_PERFORMANCE_LOG_SCHEMA.md`.

## Benchmark Summary

| Field | Value |
|---|---|
| Benchmark ID |  |
| Date/time |  |
| Stage | P7-B / P7-C / P7-D |
| Operator |  |
| Git branch |  |
| Git commit |  |
| Verdict | pass / fail / needs_review |

## Environment

| Field | Value |
|---|---|
| Unity version |  |
| Render pipeline | Built-in / URP / HDRP / unknown |
| Operating system |  |
| Test mode | Editor / Windows x64 EXE |
| Resolution |  |
| Fullscreen/windowed |  |
| Development build | yes / no / n/a |

## Machine Specs

| Field | Value |
|---|---|
| CPU |  |
| GPU |  |
| GPU driver |  |
| RAM |  |
| Storage |  |

## Scene And Asset Scope

| Field | Value |
|---|---|
| Scene/setup |  |
| Area name |  |
| Area bounds/source filter |  |
| Included categories |  |
| Excluded categories |  |
| Asset source |  |
| Import/generated size |  |
| Build size |  |

## LOD And Rendering Setup

| Field | Value |
|---|---|
| LOD policy | LOD2 / LOD3 / selected LOD4 / mixed |
| LODGroup used | yes / no |
| LOD transition notes |  |
| Collision policy | none / simple / mesh / mixed |
| Material count |  |
| Texture count |  |
| Texture compression |  |
| Batching/instancing notes |  |
| Occlusion/culling notes |  |

## Graphics Settings Observed

| Field | Value |
|---|---|
| Quality level |  |
| VSync |  |
| Target frame rate |  |
| Anti-aliasing |  |
| Shadow settings |  |
| Texture quality |  |
| Render scale |  |

## Editor Metrics

| Metric | Value | Method / Notes |
|---|---|---|
| Average FPS |  |  |
| 1% low FPS |  |  |
| Minimum FPS |  |  |
| RAM |  |  |
| VRAM / texture memory |  |  |
| Draw calls / batches |  |  |
| SetPass calls |  |  |
| Triangles |  |  |
| Vertices |  |  |
| Loading time |  |  |

## Windows EXE Metrics

| Metric | Value | Method / Notes |
|---|---|---|
| Average FPS |  |  |
| 1% low FPS |  |  |
| Minimum FPS |  |  |
| RAM |  |  |
| VRAM / texture memory |  |  |
| Draw calls / batches |  |  |
| SetPass calls |  |  |
| Triangles |  |  |
| Vertices |  |  |
| Loading time |  |  |
| Build size |  |  |

## Bottlenecks

| Bottleneck | Evidence | Severity | Follow-up |
|---|---|---|---|
| CPU |  | low / medium / high |  |
| GPU |  | low / medium / high |  |
| Memory |  | low / medium / high |  |
| Loading / streaming |  | low / medium / high |  |
| Materials / draw calls |  | low / medium / high |  |
| Textures |  | low / medium / high |  |
| Collision / physics |  | low / medium / high |  |

## Decision

| Field | Value |
|---|---|
| Pass/fail result |  |
| Reason |  |
| Accepted LOD/area policy |  |
| Required rollback | yes / no |
| Follow-up stage/task |  |
| Approved by |  |
