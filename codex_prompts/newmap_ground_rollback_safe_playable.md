# Codex Prompt: NewMap Ground Rollback Safe Playable

Implement the NewMap GroundRoadMerge rollback and safe playable ground recovery.

Requirements:
- Disable adaptive relief/DEM support grid by default.
- Restore one invisible safe support surface.
- Hide all blue/debug/support surfaces in normal mode.
- Add fall/out-of-bounds recovery to the player without warning spam.
- Preserve spawn building rejection, air walls, mouse drag, lighting, labels, and P2-P10 gameplay.
- Build `NewMapGroundRollbackPre` and parse Player.log.
- Do not claim road/terrain accuracy or full building-floating fix.
