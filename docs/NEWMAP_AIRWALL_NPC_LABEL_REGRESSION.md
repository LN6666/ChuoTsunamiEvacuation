# NewMap Airwall NPC Label Regression

Regression scope for this pass:

- Blue ground cover and fall-through prevention remain in place.
- Ground raise and player/NPC grounding remain in place.
- Spawn building avoidance remains in place.
- Boundary air walls remain active.
- Lighting/night and mouse drag behavior are not reworked.
- Official/non-official warning semantics are preserved.
- Name labels continue loading `Assets/Data/P10/newmap_name_cache.json` only.
- Runtime web requests remain disabled.

The final readiness decision is updated after preflight, Unity tests, temporary player smoke, Player.log parsing, and DeepSeek review.
