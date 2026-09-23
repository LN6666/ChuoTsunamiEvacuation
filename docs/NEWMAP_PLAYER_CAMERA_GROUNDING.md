# NewMap Player, Camera, And Grounding

P2 integration on the reset map is handled by `NewMapRuntimeBootstrap` and `NewMapPlayerController`.

Runtime behavior:

- Creates a lightweight humanoid player under `PlayerSpawnRoot`.
- Creates or reuses `Main Camera` and attaches it to a follow pivot.
- Computes map renderer bounds.
- Raycasts to scene colliders for spawn grounding.
- Creates `NewMap_RuntimeGroundSupport_DocumentedProxy` only if real scene collider grounding fails.
- Tracks last valid ground and recovers the player if endless falling is detected.

Movement policy:

| Mode | Walk | Sprint | Stamina |
|---|---:|---:|---|
| Tourism | 2.0 m/s | 10.0 m/s | Disabled |
| Evacuation | 1.0 m/s | 5.0 m/s | Enabled |

Weather modifiers:

| Weather | Modifier |
|---|---:|
| `clear_day` | 1.0 |
| `rainy_day` | 0.75 |
| `night_clear` | 0.85 |
| `night_rain` | 0.65 |

Final status: `completed_with_documented_runtime_proxy`.
