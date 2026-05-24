# P10-B++ Memory And GC Optimization Audit

## Findings

P9/P10 runtime scripts do not show per-frame JSON loading, unbounded per-frame list creation, or repeated material/mesh creation in gameplay `Update` paths.

Known memory-sensitive areas:

- High-detail scene load is scene-resident and can have a high peak.
- Runtime markers, green frames, light curtain, and crowd objects must stay capped.
- UI language refresh and rules panel should be event-driven, not per-frame.
- Background image remains a placeholder policy; no large unlicensed texture is committed.

## Low-Risk Fixes Applied

- Green frame line setup avoids temporary array allocation during activation.
- Green frame pooling can warm up before the tsunami event.
- P10-B++ frame-time metrics use a fixed-capacity ring buffer.
- P10-B metrics now report GC collection counts in summaries.
- P10-B++ performance sample includes managed heap and total allocated memory proxy fields.

## Deferred Asset Work

P10-B++ does not change texture import settings, mesh compression, lightmap settings, ProjectSettings, or URP assets. Those changes can be risky late in the project and require visual QA.

## P10-C Memory Checks

- Record process memory before scene load if possible.
- Record memory after high-detail scene is playable.
- Record memory before tsunami start.
- Record memory after green frames and light curtain are active.
- Record memory after 5 minutes.
- Record memory after scenario restart if restart is supported.
- Check Player.log for memory warnings, asset load errors, or shader errors.
