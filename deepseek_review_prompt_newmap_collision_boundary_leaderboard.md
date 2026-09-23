# DeepSeek Review Prompt: NewMap Collision Boundary Leaderboard

Review the current git diff for the Unity project `D:\UnityProjects\ChuoTsunamiEvacuation`.

Verify:
- Collision whitelist implements the user idea: only ground/support, buildings, NPC bodies/soft-blocking, and circular boundary block player movement.
- Unexpected rectangular/invisible air walls are removed or disabled.
- Route lines, shelter direct lines, green frames, labels, markers, debug objects, and tsunami hazard visuals do not block movement.
- Circular boundary is a 3.5km radius from the original map center and affects player plus NPCs.
- Player/NPC building collision and ground/fall prevention are preserved.
- Pressing R shows the shelter/ranking panel; pressing R again hides it; modal UI states gate the toggle.
- Official/non-official warning semantics are preserved and no official route validation is claimed.
- P2-P10 regressions are not introduced.
- No final release/archive is created.
- No P10-E/F/G artifacts are introduced.

Call out A-level blockers first with exact file references. If there are no A-level blockers, say so clearly and list residual risks.
