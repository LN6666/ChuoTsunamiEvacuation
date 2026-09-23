# NewMap Player NPC Collision

Player-to-NPC collision uses soft blocking with near NPC capsule triggers:

- NPC roots get a lightweight trigger `CapsuleCollider`.
- The player controller resolves movement segments against nearby NPC body radii.
- Direct overlap is corrected so the player cannot simply pass through a nearby NPC.
- Near misses can apply a bounded slowdown.
- Far NPCs do not use physical crowd collisions.
- No Rigidbody crowd pushing is used, avoiding unstable 800-NPC physics.

Tourism Mode NPCs may obstruct softly but do not cause failure. Evacuation Mode crowd delay remains bounded and is still calculated as a prototype congestion proxy.
