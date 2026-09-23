# NewMap Full Integration Plan

Confirmed plan for the Chuo_BaseMap reset integration:

1. Treat `Assets/Scenes/Chuo_BaseMap.unity` as the only active map baseline.
2. Add a runtime bootstrap that creates player, camera, UI, modes, hazards, NPCs, and documented local gameplay proxies only in `Chuo_BaseMap`.
3. Keep old P3/P4/P5 targets disabled unless a target can be anchored to the reset scene.
4. Preserve old data as evidence, not active gameplay, when no new-map anchor exists.
5. Validate with JSON status files, preflight scripts, Unity tests, optional temp build/performance sampling, and DeepSeek review.

This plan does not create a final release/archive, does not create P10-E/F/G, and does not claim official routes or GIS-grade validation.
