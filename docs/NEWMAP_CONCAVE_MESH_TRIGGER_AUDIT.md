# NewMap Concave Mesh Trigger Audit

- Active scene: `Assets/Scenes/Chuo_BaseMap.unity`
- Scene MeshCollider count: `44115`
- Scene concave MeshCollider triggers found in YAML: `0`
- Runtime source identified: `NewMapRuntimeBootstrap.AuditAndCleanupUnexpectedAirwallColliders`

The red player error was consistent with runtime cleanup converting broad interaction or unknown blocker colliders to triggers. Imported PLATEAU MeshColliders are concave, so setting `isTrigger=true` on those colliders produces Unity's `Triggers on concave MeshColliders are not supported` error.

Action taken: runtime cleanup now detects concave MeshColliders before trigger conversion. Interaction blockers receive primitive `BoxCollider` trigger proxies; unknown playable concave blockers are disabled instead of converted to triggers.

Player smoke result:

- Runtime concave MeshCollider candidates neutralized: `5835`
- Concave MeshCollider trigger errors in Player.log: `0`
- Player.log errors/warnings: `0 / 0`
