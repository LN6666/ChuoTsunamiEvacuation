# NewMap NPC Player Collision Deadlock Fix

Player-NPC collision remains enabled as soft blocking with near primitive capsules. The collision response is now local:

- Player overlap/contact increments contact diagnostics.
- Contacted NPCs choose a sidestep or local repath target.
- Other NPCs continue updating.
- Collision does not set a global NPC pause/stop flag.
- `stoppedWithoutReason` target is `0`.

The smoke pass includes a synthetic player-NPC contact resolver call and lifecycle samples.

Player smoke result:

- Player contact events: `1`
- Global respawn count after contact: `0`
- All-stop event count: `0`
- Stopped-without-reason count: `0`
- Moving count at 180 seconds: `800`
