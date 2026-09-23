# DeepSeek Review Prompt: NewMap Ground Visual Round 3

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Scope:
- Hide the blue support/collision plane in normal gameplay.
- Keep support collider functional.
- Align player/support height to visible road/building/map base samples.
- Add invisible playable-boundary air walls.
- Preserve spawn validation and left/right mouse drag look.
- Add source-name labels without fabricated road/building names.
- Add optional coordinate-based name enrichment as preprocessing only; Unity runtime must not make web requests.
- Preserve lighting/night behavior without reworking it.
- Preserve P2-P10 gameplay systems and official/non-official warning semantics.

Verify:
- Support proxy is collider-only or renderer-hidden in normal mode.
- Air walls use invisible `BoxCollider`s and are integrated with spawn/NPC bounds.
- Spawn validation still rejects building overlap and out-of-bounds candidates.
- Left and right drag look still work; mouse movement alone does not rotate.
- Label runtime reads local source/cache only and does not fabricate names.
- Online lookup tool is not used at runtime, is rate-limited, cached, and documented.
- Low-confidence/address-only online results are hidden in normal gameplay.
- No false official shelter/route/safety claims.
- No final release/archive, no P10-E/F/G, no P11 move.
- No A-level blockers remain.

Validation artifacts to inspect:
- `docs/NEWMAP_BLUE_GROUND_DIAGNOSIS.md`
- `docs/NEWMAP_SUPPORT_SURFACE_VISIBILITY_FIX.md`
- `docs/NEWMAP_GROUND_VISUAL_ALIGNMENT_ROUND3.md`
- `docs/NEWMAP_PLAYABLE_BOUNDS_AIR_WALL.md`
- `docs/NEWMAP_OBJECT_NAME_LABEL_SOURCE_REPORT.md`
- `docs/NEWMAP_NAME_ENRICHMENT_FROM_COORDINATES.md`
- `Assets/Data/P10/newmap_ground_visual_alignment_round3.json`
- `Assets/Data/P10/newmap_playable_bounds_report.json`
- `Assets/Data/P10/newmap_name_cache.json`
- `Assets/Data/P10/newmap_ground_visual_round3_player_report.json`
- `Assets/Data/P10/newmap_ground_visual_round3_player_log_summary.json`

Return:
- A-level blockers, if any.
- B/C issues and suggested small fixes.
- Whether this is ready for another manual visual playtest with documented limitations.
