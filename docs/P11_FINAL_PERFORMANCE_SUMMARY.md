# P11 Final Performance Summary

Status: passed with documented startup/stutter limitations.

The final performance report records average FPS, max frame time, stutter frames, memory sampling, NPC state counts, and Player.log cleanliness.

Final sample:

- Duration: `180` seconds
- Average FPS: `425.13`
- Max frame time: `1036.11 ms`
- Stutter frames over 66ms: `2`
- Active NPC count: `1600`
- NPCs stopped without reason: `0`
- Player.log errors: `0`
- Player.log warnings: `0`

Known limitation: first startup and map initialization can produce visible loading/stutter spikes. This is documented in `KNOWN_LIMITATIONS.md`.
