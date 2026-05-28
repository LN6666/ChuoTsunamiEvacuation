# NewMap Name Label Runtime Report

Generated: 2026-05-29T00:00:00+09:00

Runtime label system:
- Uses world-space `TextMesh` labels.
- Reads local config/cache only.
- Shows existing official shelter and non-official candidate names.
- Can show cached road/building names only if preprocessing wrote reliable source names.
- Hides low-confidence and ID-only entries in normal mode.

Performance safeguards:
- Max visible labels: 80
- Road/building category caps
- Distance and frustum culling
- Screen-spacing declutter
- Throttled label refresh
- No full-scene scan every frame
- No runtime network requests

Current generic road/building status: `no source name available`

Player smoke:
- Available labels: 93
- Active labels: 8
- Official shelter labels: 15
- Non-official candidate labels: 78
- Road name labels: 0
- Building name labels: 0
- ID-only labels: 0
- Runtime network requests: false
