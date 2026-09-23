# NewMap NPC 180s Lifecycle Smoke

The runtime smoke coroutine records NPC lifecycle samples at approximately 10, 30, 60, and 180 seconds when `-newmapSelfAuditSmoke` is used.

Tracked fields:

- active NPC count
- created at startup
- global respawn count
- individual respawn count
- pool recycle count
- destroy and instantiate-after-startup counts
- all-stop event count
- player contact events
- stopped-without-reason count
- moving, arrived, queued, stuck, and static counts

Final 180-second result:

- Active NPCs: `800`
- Created at startup: `800`
- Global respawn count: `0`
- Destroyed during smoke: `0`
- Instantiate after startup: `0`
- All-stop event count: `0`
- Player contact events: `1`
- Stopped-without-reason count: `0`
- Moving count: `800`
- Average speed: about `0.823 m/s`
