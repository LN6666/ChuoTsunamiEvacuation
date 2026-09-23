# NewMap Unexpected Airwall Hard Audit

This pass audits all runtime colliders that can block the player: boundary air walls, invalid-zone blockers, gameplay ground cover, building obstacle bounds, NPC body triggers, target interactions, debug/test colliders, and unknown invisible blockers.

- Boundary air walls are preserved.
- Gameplay ground cover colliders are preserved.
- NPC body colliders are triggers and feed soft blocking, not rigid-body crowd physics.
- Building obstacle bounds are now restricted to building-like renderers and oversized/non-semantic bounds are filtered.
- Runtime player logs include `airwallHardCollidersScanned`, `unexpectedAirwallBlockers`, `boundaryAirWallsPreserved`, and `sampledValidPathsPassable`.

The initial JSON report is updated by the player log parser after the temporary build smoke.
