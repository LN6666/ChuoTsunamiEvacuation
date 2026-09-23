# P10-B++ Stutter Reduction Plan

## Likely Spike Moments

- High-detail scene load.
- Tsunami start.
- Green ground frame appearance.
- Light curtain activation.
- Crowd/NPC spawn or congestion setup.
- ResultPanel opening.
- Language switch.
- Rules UI opening.
- Night overlay activation.

## Mitigations Applied

- Green frame pooled objects can warm before tsunami start.
- Green frame setup avoids per-frame activation array allocation.
- Debug layers and debug labels remain off by default.
- Runtime layer toggles can disable marker, crowd, light curtain, or debug roots for diagnosis.
- P10-B++ metrics count frames over a spike threshold.

## Manual Mitigation Policy

If P10-C profiling shows spikes:

- First compare with debug roots off.
- Then compare green frames off/on.
- Then compare light curtain off/on.
- Then lower NPC/marker/frame caps through quality preset.
- Only after evidence should larger loading or render pipeline changes be considered.

## Not Implemented In P10-B++

- Full async production chunk streaming.
- Addressables.
- AssetBundles.
- Jobs/Burst/ECS rewrite.
- PLATEAU asset mutation.
- Scene splitting.
