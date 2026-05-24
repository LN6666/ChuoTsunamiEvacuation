# P10-B++ CPU Optimization Audit

## Hotpath Scan

P9/P10 runtime scripts were scanned for common CPU risks:

- `FindObjectsOfType` in `Update`: not found.
- `GameObject.Find` in `Update`: not found.
- per-frame JSON parsing: not found.
- LINQ in `Update`: not found.
- per-frame material or mesh creation in P9/P10 `Update`: not found.
- repeated marker or green-frame regeneration in `Update`: not found.

Existing `Update` methods are narrow:

- `P10BPerformanceMetricsRecorder.Update` records frame time only when enabled.
- `P10BPlusMenuRuntime.Update` checks `Escape`.
- `P9CrowdRuntimeAgent.Update` moves runtime agents.

## Low-Risk Fixes Applied

- Green frame activation no longer allocates a new `Vector3[]` for each configured frame.
- Green frame pooled objects can be prewarmed before tsunami start.
- P10-B++ runtime optimizer can keep debug roots disabled by default.
- P10-B++ metrics use a fixed-capacity ring buffer for bounded frame-time history.

## Remaining CPU Risks

- Authoritative CPU usage is not available from safe Unity runtime code in this sprint.
- High-detail rendering cost and city-object count must be measured in P10-C with the Unity Profiler and Windows tools.
- Crowd/NPC movement should remain capped; P10-B++ does not introduce thousands of NPCs.

## P10-C CPU Profiling Targets

- Baseline idle in high-detail scene.
- Tsunami start.
- Green frame appearance.
- Light curtain appearance.
- Crowd/congestion scenario.
- ResultPanel opening.
- Language switch and rules UI opening.
- Night/rain mode.
