# P8-A New Map Smoke Test Plan

## Purpose

Confirm that P2-P6 systems can run against `P7_HighDetail_Chuo.unity` without hard dependencies on the legacy base map.

## Smoke Areas

- Open the accepted high-detail scene.
- Verify a player spawn can be staged without changing success/failure rules.
- Verify camera framing can be staged.
- Verify shelter marker placement can target high-detail map coordinates or proxy anchors.
- Verify result panel flow remains independent of map source.
- Verify P5 overlays and qualified shelter display remain opt-in.
- Verify P6 navigation guidance can target staged objects.
- Verify NPC prototype can be staged as a prototype only, not as P9 crowd behavior.

## P8-A Result

P8-A provides the gate and tooling. Full runtime smoke on the populated scene remains a follow-up because P8-A is data-layer foundation only.
