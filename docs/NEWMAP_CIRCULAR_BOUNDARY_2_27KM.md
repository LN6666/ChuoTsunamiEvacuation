# NewMap Circular Boundary 2.27km

Updated: 2026-05-30T23:00:00+09:00

Requirement:
- Center source: original Chuo_BaseMap map center.
- Runtime center from current diagnostics: `-2.14, 474.58`.
- Radius: `2270m`.
- Boundary height: `300m`.
- Normal gameplay visibility: off.
- Debug visualization: off by default.
- Player clamp: enabled.
- NPC clamp: enabled.

Dependent systems:
- Player movement uses the runtime circular clamp.
- NPC spawn and movement use the same circular boundary.
- Random spawn and safe fallback validation reject points outside the circle.
- Active targets outside the circle are disabled before UI, green frames, entry triggers, or route guidance are configured.
- Route/prototype guidance is suppressed for disabled out-of-bounds targets.
- Old rectangular air-wall colliders are not created for normal gameplay.

Manual checks:
- Walk toward the expanded map edge; the player should be blocked at the invisible 2.27km circle.
- NPCs should spawn and remain inside the same circle.
- No visible circular wall, blue boundary, or rectangular inner air wall should appear in normal gameplay.
- Debug boundary visualization should remain hidden unless an explicit debug flag enables it.
