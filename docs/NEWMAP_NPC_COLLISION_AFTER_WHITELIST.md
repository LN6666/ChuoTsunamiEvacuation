# NewMap NPC Collision After Whitelist

NPC collision remains a required blocker through the existing soft-blocking system.

Runtime behavior:
- NPCs keep near-body capsule triggers.
- Player movement applies local correction/slowdown near NPC bodies.
- NPC contact does not trigger global NPC refresh or all-stop behavior.
- NPC positions are clamped inside the 3.5km circular boundary.

The player should not pass directly through NPCs, but should still be able to move around them.
