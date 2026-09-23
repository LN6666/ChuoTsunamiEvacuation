# NewMap Building Bounds Spawn Avoidance

Generated: 2026-05-29T00:00:00+09:00

The spawn blocker was caused by candidate placement using broad map bounds without rejecting building footprints. The fix adds a lightweight startup-only building avoidance cache.

Rules:

- Cache is built once during `NewMapRuntimeBootstrap` scene startup.
- Runtime-generated UI, markers, support proxies, debug roots, labels, and line renderers are excluded.
- Bounds smaller than building-like geometry or larger than map-layer footprints are ignored.
- The cache is used only during spawn selection, not every frame.
- Spawn candidates inside cached building renderer bounds are rejected.
- Spawn candidates closer than `minDistanceFromBuildingMeters` are rejected.

Limitations:

- This does not infer official road surfaces.
- Renderer bounds are conservative boxes, so some valid narrow spaces near buildings may be rejected.
- If PLATEAU import combines many buildings into a single oversized renderer, that renderer is skipped to avoid making the whole map unspawnable.

Latest player smoke:

- Cached building bounds: 12,742
- Spawn candidates rejected inside building: 2
- Accepted spawn nearest building distance: 4.70m
