# DeepSeek Review Prompt: NewMap Building Snapdown + NPC 100x

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Verify:

- Floating building snapdown uses the accepted gameplay ground cover as the visual/gameplay reference.
- Building snapdown does not blindly move roads, ground cover, water, support colliders, player, NPCs, UI, air walls, or markers.
- Building roots are lowered safely while preserving X/Z, rotation, and scale.
- Official shelter markers, non-official candidate markers, green frames, E interaction zones, and route proxies remain aligned after snapdown.
- NPC count request is 100x the 8-NPC baseline, with an explicit cap, pooling/build-once reuse, sector/ring distribution, building avoidance, playable-bounds checks, and far NPC throttling/static proxy behavior.
- Tourism Mode NPCs do not cause failure.
- Evacuation Mode crowd delay remains bounded.
- Blue ground cover, fall prevention, spawn validation, mouse drag look, day/night lighting, official/non-official warning semantics, and P2-P10 gameplay are not regressed.
- Runtime web requests remain disabled.
- The work does not claim GIS-grade PLATEAU/terrain/building elevation accuracy.
- No final release/archive is created.
- No P10-E/F/G artifacts are created.

Flag A-level blockers if any of the above is unsafe.
