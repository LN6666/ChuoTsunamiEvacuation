# P8-C New Map Gameplay Smoke Results

Date: 2026-05-24.

## Baseline

Practical baseline scene:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

The scene existed before P8-C and had local dirty state before P8-C work began. P8-C did not reset, checkout, overwrite, delete, or stage this scene.

Legacy fallback:

`Assets/Scenes/Chuo_BaseMap.unity`

The legacy fallback was not present in this workspace path and had no git status entry. P8-C did not modify it.

## Smoke Coverage

P8-C PlayMode tests cover temporary-scene smoke behavior:

- infrastructure target proxy can exist in a temporary scene
- hazard evaluator updates a target state over time
- risk-front controller and infrastructure evaluator coexist
- navigation target proxy works without `Chuo_BaseMap.unity`
- shelter interaction proxy exists without changing result rules
- P6 NPC prototype can stage against a limited proxy target
- P9 systems are not required

## Scene Anchor Decision

P8-C did not mutate existing high-detail map objects.

Because the high-detail scene was already locally dirty, P8-C scene anchoring was deferred to documented/manual setup rather than forced into the scene file. Runtime proxy evidence is provided through scene-safe components and automated temporary-scene tests.
