# NewMap NPC Continuous Movement Fix

NPC movement now tracks explicit states:
- Moving
- WaitingAtCrossingOrCrowd
- QueuedAtEntrance
- Arrived
- Repathing
- StuckRecovering
- PausedByMode
- StaticFarProxy

Tourism Mode NPCs continue ambient wandering. Evacuation Mode keeps bounded crowd delay and does not make NPCs directly kill the player.

If an NPC stops without a valid state for too long, stuck recovery picks a nearby valid ground-cover position and resumes movement. Far update throttling remains enabled, but static far proxy mode is disabled by the movement config for this pass so NPCs do not silently freeze.

Validated player-smoke summary:
- active NPCs: 800
- moving/arrived/stuck/static: 791 / 9 / 0 / 0
- stopped without reason: 0
- average NPC speed: 0.785m/s
- recovered/repositioned events during smoke: 4,378
- average FPS sample after spatial building-bound indexing: 213.19
