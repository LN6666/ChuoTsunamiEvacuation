# NewMap Manual Playtest Readiness

Generated: 2026-05-29T00:00:00+09:00

Decision: `ready_with_documented_visual_limitations`

Reason: spawn-on-playable-ground validation, building-bounds rejection, left/right mouse drag, preflight, EditMode, PlayMode, temp player build, Player.log parse, and DeepSeek review passed. Manual visual confirmation is still required for visible road/playable-ground feel and previously documented material/ground limitations.

- Mouse look: `left_and_right_drag_look_validated_player_smoke_manual_confirmation_remaining`
- Spawn validation: `road_or_playable_ground_only_spawn_validated_3_attempts_2_building_rejections_nearest_building_4_70m`
- Lighting: `round2_day_preserved_night_sky_dark_buildings_readable_by_profile_and_smoke_manual_confirmation_remaining`
- Debug cleanup: `production_mode_debug_objects_hidden_by_preflight_and_player_log_manual_visual_confirmation_remaining`
- Material/LOD visual: `round2_material_texture_audit_documented_no_lod2_claim_manual_acceptance_remaining`
- Ground alignment: `round2_support_raised_to_1_97m_sampled_visual_base_spawn_delta_0_04m_manual_confirmation_remaining`
- Building clipping: `round2_support_height_realign_done_two_elevated_active_target_offsets_documented_manual_classification_remaining`
- NPC distribution: `validated_160_requested_160_spawned_within_1000m_24_sectors_5_rings_no_cap`
- Gameplay regression: `EditMode_114_114_PlayMode_27_27_passed`
- Temp player: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapSpawnMouseFixPre\ChuoTsunamiEvacuation_NewMapSpawnMouseFixPre.exe`
- Player.log: 0 errors, 0 warnings; spawn smoke passed; left/right drag smoke passed
- Ground smoke: old support 0.00m, new support 1.97m, visual ground 1.97m, spawn delta 0.04m, 2 active target height-offset violations
- Night smoke: sky brightness 0.042, building readability score 0.517, day restored
- FPS/stutter sample: 180s, 160 NPCs active, 3324.68 average FPS in hidden smoke run, 2 frames over 66ms
- EditMode: passed 114/114
- PlayMode: passed 27/27
- Preflight: passed
- DeepSeek: passed, no A-level blockers

Remaining limitations:

- LOD2 textured building quality is not claimed because the documented current import is Buildings/LOD1.
- Photo-projected/appearance texture roughness is documented as an import/PLATEAU material limitation unless a later approved material conversion or reimport proves otherwise.
- A single flat gameplay support plane cannot perfectly match every elevated terrain/building base across the full Chuo map; 2 active target anchors remain more than 2.5m from the support height and require manual visual classification.
- Physical mouse drag feel and cursor behavior still need confirmation in the manual build.
- Manual visual confirmation is still required for night building readability and ground/building alignment from the player camera.
- Manual spawn confirmation is required to verify the player starts on visible road/playable ground and can move immediately.
