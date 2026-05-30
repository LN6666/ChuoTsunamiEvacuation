# NewMap NPC Double Count

The previous active NewMap NPC target was `800` from baseline `8 * 100`. This pass doubles the requested and capped count:

- Previous requested/cap: `800`
- New requested/cap: `1600`
- Multiplier: `200`
- Pooling: enabled
- Far NPC update throttling: enabled
- Far NPC static proxy: enabled
- Near player/NPC soft blocking distance: `60m`

If runtime performance becomes unacceptable, the cap must be lowered and reported. The current config attempts the requested full doubling.

Runtime retest evidence:
- Requested/spawned/capped NPCs: `1600/1600/1600`
- Cap status: not capped
- Active 180-second sample: average FPS `1767.61`, max frame `1021.96ms`, stutter frames over 66ms `2`
- Diagnostic warmup max frame: `14286.81ms`, documented separately as self-audit/startup warmup
- NPC state sample: moving `11`, static far proxies `1589`, stopped-without-reason `0`
