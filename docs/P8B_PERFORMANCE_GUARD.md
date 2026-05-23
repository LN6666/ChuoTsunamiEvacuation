# P8-B Performance Guard

P8-B risk-front visualization must remain bounded before any visual scene implementation is accepted. This guard is for early warning only; it does not add packages, scene objects, or runtime visual behavior.

## Guarded Risks

- Excessive segment counts can create too many vertices, line sections, or mesh elements.
- Expensive per-frame mesh rebuild is not acceptable for the default path.
- High transparency overdraw is likely when a light curtain covers dense PLATEAU city geometry.
- Too many light curtain objects can add transform, renderer, sorting, and batching cost.
- Unbounded particle usage is prohibited.
- Material/shader risk must be reviewed before using multi-pass, complex transparent, or high-cost shader paths.
- Lack of culling or enable/disable strategy is a blocker for visual implementation readiness.

## Current Budget Defaults

The data-only P8-B guard config uses:

- `segmentCount`: 128
- `rebuildsMeshEveryFrame`: false
- `transparentLayerCount`: 1
- `lightCurtainObjectCount`: 1
- `maxParticleCount`: 0
- `particlesAreBounded`: true
- `materialMode`: `transparent_unlit`
- `shaderRisk`: `low`
- `hasCullingStrategy`: true
- `hasEnableDisableStrategy`: true
- `usesSharedMeshOrInstancePool`: true

These defaults are placeholders for a future visual implementation review. They are not scene wiring and not proof of final frame-rate performance.

## Tooling

Run:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p8/inspect_p8b_visual_performance_risk.ps1
```

The tool warns about unsafe values. `tools/p8/run_p8b_guard_preflight.ps1` runs it with warnings treated as errors so the guard branch cannot accidentally accept a high-risk visual budget.

## Acceptance Rule

P8-B visual work must keep performance settings bounded, avoid per-frame mesh rebuild, document culling/disable strategy, and keep particles optional and bounded. This branch does not implement the visual light curtain scene objects.
