# Codex Task Prompt: NewMap Collision Boundary Leaderboard

Implement and validate the P10 NewMap collision whitelist, 3.5km circular boundary, and R leaderboard toggle.

Constraints:
- Do not create a final release/archive.
- Do not move to P11 or create P10-E/F/G.
- Do not reimport map data.
- Preserve P2-P10 gameplay behavior.
- Do not claim official route validation.

Acceptance:
- Player blocked only by ground/support, buildings, NPC soft bodies, and circular boundary.
- NPCs stay inside the circular boundary.
- Direct shelter lines, green frames, labels, markers, and hazard visuals are nonblocking.
- R shows and hides the ranking UI safely during gameplay.
- EditMode, PlayMode, preflight, temporary player build, Player.log parse, and DeepSeek review are reported.
