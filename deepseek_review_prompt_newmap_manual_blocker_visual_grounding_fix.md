# DeepSeek Review Prompt - NewMap Manual Blocker Visual/Grounding/NPC Fix

Review the current git diff for the NewMap manual playtest blocker fix.

Verify:

- The manual blocker list is acknowledged in docs/data.
- Mouse look is restored for Tourism and Evacuation modes.
- Cursor lock/unlock is separated between gameplay and menu/pause/result states.
- Lighting/material darkness is addressed without false LOD2 claims.
- Debug/test objects are hidden in normal manual mode.
- `DebugDiagnosticsRoot` is inactive by default and diagnostics-only objects are not production-visible.
- Ground/support alignment is addressed and support visuals do not clip buildings.
- Building clipping is fixed where runtime-caused and classified where imported-data-caused.
- NPC distribution requests 20x previous count, applies a safe cap, spreads broadly within 1000m, and is deterministic.
- NPC implementation avoids heavy pathfinding/per-frame explosion.
- Tourism Mode has ambient NPCs but no crowd failure.
- Evacuation crowd delay remains bounded and explainable.
- Official/non-official warning semantics are preserved.
- Route wording does not claim official evacuation routes.
- Tourism/Evacuation modes, two-stage tsunami, green frames, light curtain, ResultPanel, E interaction, and disabled-target behavior remain intact.
- No final release/archive was created.
- No P10-E/F/G milestone was introduced.
- No A-level blockers remain.

Focus on Unity lifecycle risks, NullReference risks, compile risks, scene/runtime bootstrap behavior, and whether the docs/status JSON overclaim manual-visible success.
