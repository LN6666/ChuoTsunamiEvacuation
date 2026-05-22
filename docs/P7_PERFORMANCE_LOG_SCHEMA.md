# P7 Performance Log Schema

Date: 2026-05-22

Stage: P7-A

Status: Required Markdown field schema for benchmark records.

## Purpose

P7 benchmark records are Markdown-first. The validator checks for required field names and sections only; it does not parse numeric values yet.

Use:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/validate_p7_performance_log.ps1 -Path docs/p7_benchmark_records/example.md
```

## Required Fields

| Field | Required | Notes |
|---|---|---|
| Timestamp | Yes | Record creation or benchmark run time. |
| Branch | Yes | Git branch used for the run. |
| Machine specs | Yes | CPU, GPU, RAM, OS, storage where available. |
| Scene/setup | Yes | Scene, temporary setup, or benchmark harness. |
| Asset scope | Yes | Area, source folders, categories, and size assumptions. |
| LOD level | Yes | LOD2, LOD3, selected LOD4, or mixed policy. |
| Graphics settings | Yes | Quality level, resolution, VSync, shadows, texture settings, and related notes. |
| Editor Metrics | Yes | Section for Unity Editor measurements. |
| Windows EXE Metrics | Yes | Section for player-build measurements or explicit placeholder. |
| Average FPS | Yes | Required for Editor and EXE when measured. |
| 1% low FPS | Yes | Required for frame stability tracking when measured. |
| RAM | Yes | Process or system RAM notes. |
| VRAM / texture memory | Yes | GPU or Unity texture memory notes where available. |
| Draw calls | Yes | Unity Stats, profiler, or frame debugger source. |
| Batches | Yes | Unity Stats, profiler, or frame debugger source. |
| Triangles | Yes | Representative camera value. |
| Loading time | Yes | Time to enter usable state. |
| Build size | Yes | Required for Windows EXE benchmarks; placeholder allowed before EXE exists. |
| Bottlenecks | Yes | CPU, GPU, memory, loading, material, texture, collision, or unknown. |
| Decision | Yes | Pass, fail, needs review, rollback, or deferred. |
| Notes | Yes | Method limits, manual steps, uncertainties, and follow-up. |

## Placeholder Policy

Placeholders are allowed in skeleton records and early P7-A/P7-B prep, but benchmark decisions must not rely on placeholders. Any placeholder metric used in a decision must be marked `needs_review` or deferred.

## Validator Scope

`tools/p7/validate_p7_performance_log.ps1` checks that required labels exist in the Markdown. It intentionally does not validate units, numeric ranges, FPS calculation method, profiler source, or EXE build provenance yet.
