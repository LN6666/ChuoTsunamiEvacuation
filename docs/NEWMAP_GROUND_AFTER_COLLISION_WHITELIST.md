# NewMap Ground After Collision Whitelist

Ground/support collision is preserved.

Runtime behavior:
- Visible road-like gameplay ground cover remains active and collidable.
- The invisible support surface remains non-rendered but collidable.
- The support surface is expanded to cover the 3.5km circular boundary.
- Fall recovery remains enabled.
- Spawn grounding remains tied to the gameplay support surface.

This keeps the player from falling while removing unrelated visual/helper blockers.
