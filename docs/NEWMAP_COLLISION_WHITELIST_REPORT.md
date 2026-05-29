# NewMap Collision Whitelist Report

Status: validated by EditMode, PlayMode, preflight, temporary player smoke, and Player.log parse.

Movement blockers now follow the whitelist:
- Ground/support remains collidable for fall prevention.
- Building obstacle collision remains enabled.
- NPC bodies remain soft-blocking.
- Map edge uses the circular boundary clamp.

Everything else is visual or trigger-only. Shelter direct lines use `LineRenderer` only and create zero colliders. Route lines, green frames, labels, markers, debug objects, and hazard visuals are disabled or converted if they would physically block the player.

No official route validation is claimed. The direct-line/ranking display is gameplay guidance only.

Player smoke result:
- Direct shelter lines: 95
- Direct-line colliders: 0
- Old rectangular air-wall colliders: 0
- Unknown blockers inside playable area: 0
- Route/green-frame/label/hazard visual blockers: 0
