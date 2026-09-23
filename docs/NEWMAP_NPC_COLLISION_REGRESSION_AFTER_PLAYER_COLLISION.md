# NewMap NPC Collision Regression After Player Collision

The player-NPC collision pass keeps the existing NPC lifecycle safeguards:

- 100x NPC distribution remains capped at 800.
- NPCs continue using building-bound avoidance and stuck recovery.
- NPCs do not use heavy per-NPC pathfinding.
- Near-player collision uses soft body resolution instead of rigid-body crowd physics.
- Tourism Mode does not fail because of NPC contact.
- Evacuation Mode crowd delay remains bounded.

Player smoke and PlayMode tests verify that NPC body colliders exist, soft blocking is enabled, and stopped-without-reason remains zero.
