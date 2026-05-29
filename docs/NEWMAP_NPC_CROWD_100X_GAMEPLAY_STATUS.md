# NewMap NPC Crowd 100x Gameplay Status

Tourism Mode keeps NPCs as ambient proxies and does not use them as a failure condition.

Evacuation Mode keeps crowd delay bounded. NPCs do not directly kill the player, and the crowd-delay target continues to use aggregate metrics instead of expensive pathfinding for every NPC.
