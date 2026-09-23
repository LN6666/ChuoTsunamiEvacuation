# P7-C Benchmark Results

## Current Result Status

P7-C implementation results are recorded here.

## Import Inspection

Command:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/inspect_p7c_benchmark_import.ps1
```

Result:

| Metric | Value |
|---|---:|
| Non-meta files | 5843 |
| Bytes | 634782243 |
| Size | 605.38 MB |
| `.gml` files | 6 |
| `.jpg` files | 5837 |
| Renderable Unity/model assets detected | 0 |

Conclusion: `53393690` remains raw CityGML plus texture source files. It is not yet verified as renderable Unity mesh production content.

## Logical Group Summary

| Group | Files | Bytes | Size MB | Renderable Assets |
|---|---:|---:|---:|---:|
| bldg | 5181 | 272392412 | 259.77 | 0 |
| brid | 43 | 16122211 | 15.38 | 0 |
| fld | 1 | 17335453 | 16.53 | 0 |
| frn | 616 | 269508019 | 257.02 | 0 |
| tran | 1 | 46558793 | 44.40 | 0 |
| veg | 1 | 12865355 | 12.27 | 0 |

## Metrics Harness

`P7BenchmarkMetricsRecorder` now includes:

- sample count
- elapsed seconds
- average FPS
- approximate 1 percent low FPS
- average frame milliseconds
- active chunk count
- chunk binding count
- imported candidate file count
- imported candidate byte count
- chunk enabled/disabled state summary

These values are approximate telemetry and not Unity Profiler replacements.

## Validation Results

| Check | Result |
|---|---|
| P7-C preflight | PASS |
| Unity GUI EditMode tests | PASS - 151/151 |
| Unity GUI PlayMode tests | PASS - 30/30 |
| DeepSeek review | CONDITIONAL PASS - no A-level blockers |
| Protected paths | PASS - `Chuo_BaseMap.unity`, `Assets/PLATEAU`, `Assets/Data`, `ProjectSettings`, and `Packages` clean after reverting Unity-generated ProjectSettings churn |

## Unity Notes

The P7-C scene/registry builder succeeded in Unity GUI mode.

Unity batchmode scene build failed in this environment because package registration was blocked by licensing/headless entitlement behavior and Unity registered 0 packages. GUI mode was used for the required scene build and automated tests.

## DeepSeek Follow-Ups

DeepSeek reported no A-level blockers.

B-level follow-ups:

- Treat batchmode scene build as optional until a reliable headless Unity licensing/package workflow is confirmed.
- Keep metrics documented as approximate telemetry and complete authoritative profiling in P7-D.
- Keep full async/production streaming deferred; P7-C is synchronous placeholder enable/disable only.
- Track repeated P7-C scene-builder material cleanup as a minor follow-up if the builder is run many times.

## Protected Path Statement

P7-C must keep these paths unchanged:

- `Assets/Scenes/Chuo_BaseMap.unity`
- `Assets/PLATEAU/`
- `Assets/Data/`
- `ProjectSettings/`
- `Packages/`
- production scenes outside `Assets/Scenes/P7Benchmark/`
- existing gameplay scripts outside `Assets/Scripts/P7Benchmark/`
