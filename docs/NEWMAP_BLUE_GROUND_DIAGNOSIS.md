# NewMap Blue Ground Diagnosis

Generated: 2026-05-29T00:00:00+09:00

Manual issue: a large blue road/ground-like surface was visible in normal gameplay.

Diagnosis:
- The runtime support/collision surface is the primary suspect.
- Normal gameplay must never render support/collision/debug ground.
- Imported road/building visuals are not disabled by color alone.

Runtime fix:
- `NewMap_RuntimeGroundSupport_DocumentedProxy` is now created as a `BoxCollider`-only object.
- `GameplaySupportRoot` renderers are disabled in normal mode.
- Debug/support ground candidates with support/debug names are hidden.

Status: `implemented_pending_player_smoke`
