# P7-C New Map Gameplay Smoke Test

## Purpose

This smoke test validates that the new high-detail scene can host P2-P6 gameplay systems after actual PLATEAU assets are loaded.

## Preconditions

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` exists.
- Actual high-detail PLATEAU assets are loaded, not only metadata placeholders.
- Required shelter and navigation test objects are staged.
- No changes were made to gameplay success/failure rules.

## Smoke Checklist

1. Open `P7_HighDetail_Chuo.unity`.
2. Add or enable a player spawn on a safe walkable test location.
3. Confirm player movement and camera control work.
4. Confirm a shelter interaction can be represented with a test shelter object.
5. Confirm result panel can display success/failure without depending on the old scene.
6. Confirm P5 data loaders run and do not require `Chuo_BaseMap.unity`.
7. Confirm P6 navigation guidance can target staged objects in the new scene.
8. Confirm P6 NPC prototype can be staged while P9 crowd or real spawn systems remain not allowed.
9. Confirm the smoke test must not implement P8 hazard, inundation, light curtain, or risk-front systems.
10. Record pass/fail notes in `docs/P7C_TEST_RESULTS.md`.

## Current Status

P7-C prepares the scene shell and compatibility markers. Full runtime smoke testing is pending actual high-detail asset import and P7-D profiling preparation.
