# NewMap Concave Mesh Trigger Fix

- Rule enforced: concave `MeshCollider` objects are never used as triggers.
- Existing concave MeshCollider triggers are neutralized to non-trigger.
- Runtime interaction proxies use primitive `BoxCollider` triggers.
- Support, building, and air-wall colliders remain non-trigger or primitive.

Player-log validation passed in the temporary player build:

- Concave MeshCollider trigger errors: `0`
- Runtime offenders neutralized: `5835`
- Primitive trigger proxies needed: `0`
- Player.log errors/warnings: `0 / 0`

This preserves player/building collision, NPC/building avoidance, and air-wall boundaries while removing the runtime error path.
