# P7 Benchmark Automation Plan

Date: 2026-05-22

Stage: P7-A

Status: Automation preparation only. No Unity benchmark scene, asset import, or player build is created by this plan.

## Purpose

P7 benchmark work should be repeatable from the command line where practical, with Markdown records as the first source of truth. P7 has exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D.

## Command-Line Helpers

P7-A adds these helpers:

- `tools/p7/new_p7_benchmark_record.ps1` creates a timestamped Markdown benchmark skeleton under `docs/p7_benchmark_records/`.
- `tools/p7/validate_p7_performance_log.ps1` checks that a benchmark Markdown record contains required fields.
- `tools/p7/run_p7_benchmark_preflight.ps1` runs the base P7 preflight and optionally creates or validates a benchmark record.

Default benchmark preflight:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_benchmark_preflight.ps1
```

Create and validate a skeleton record:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_benchmark_preflight.ps1 -CreateRecord -Stage P7-B
```

Validate an existing record:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_benchmark_preflight.ps1 -ValidateRecordPath docs/p7_benchmark_records/example.md
```

## Editor Benchmark Automation

P7-B should start with Editor measurements for a small approved area. The Editor benchmark can remain partly manual until a Unity benchmark runner exists, but the record must capture:

- scene or temporary setup
- asset scope
- LOD level
- graphics settings
- sample route or fixed camera points
- average FPS and 1% low FPS
- memory, draw calls, batches, triangles, and loading time
- bottlenecks and decision

If a later stage adds a Unity-side benchmark runner, it must be approved by a Markdown plan and must stay outside P7-A.

## Windows EXE Benchmark Automation

Windows x64 EXE measurements matter because Editor overhead can hide or exaggerate player-build bottlenecks. P7-B may run an optional EXE benchmark when a build already exists. P7-D must include Windows EXE profiling before closeout.

The EXE benchmark record should capture:

- build date/time and build size
- development build status
- resolution and fullscreen/windowed mode
- machine specs and GPU driver if available
- average FPS and 1% low FPS
- RAM and VRAM or texture memory
- loading time
- observed CPU, GPU, memory, material, texture, or streaming bottlenecks

## GUI Automated Tests

Automated Unity GUI tests are required when a P7 stage changes Unity code, Unity assets, Unity scenes, gameplay-facing data, packages, or project settings.

P7-A Codex B is docs/tools/prompts only. Unity tests are intentionally not required for this work unless unrelated Unity file changes appear in the diff.

## Manual-Only Steps

Manual steps are acceptable only when the project does not yet expose a reliable command-line source for the metric. Examples:

- Unity Stats window readings
- Frame Debugger observations
- GPU driver tool readings
- subjective visual quality checks

Manual values still belong in the Markdown record with method notes.

## Markdown-First Record Policy

Each benchmark run should produce one Markdown record before decisions are finalized. Raw profiler captures, screenshots, or logs are supporting evidence only and should not be committed if they are large or generated.
