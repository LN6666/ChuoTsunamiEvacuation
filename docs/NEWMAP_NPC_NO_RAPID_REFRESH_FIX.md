# NewMap NPC No Rapid Refresh Fix

- NPC generation is session/mode-start only.
- Global refresh is disabled: `allowGlobalRefresh=false`.
- Global respawn interval is `0`.
- Far NPC despawn is disabled.
- Stuck recovery repositions only individual invalid/stuck NPCs.
- Runtime counters report startup creation, destroy, instantiate-after-startup, pool recycle, and global respawn counts.

Player smoke result:

- Created at startup: `800`
- Global respawn count: `0`
- Destroyed during smoke: `0`
- Instantiate after startup: `0`
- Pool recycle count: `0`

No global refresh or visible full-list regeneration was detected in the 180-second lifecycle smoke.
