# NewMap Manual Playtest Readiness

Generated: 2026-05-29T00:00:00+09:00

Decision: `ready_with_documented_visual_limitations`

Reason: Round 3 preflight, Unity tests, temp player build, player smoke, and Player.log parse passed. Support/road/building reference deltas are 0.00m in the runtime diagnostics, the support collider is invisible, playable air walls are present, spawn/mouse regressions pass, and labels remain offline/source-only. Manual visual confirmation is still required from the player camera.

- Mouse look: `left_and_right_drag_look_validated_player_smoke_manual_confirmation_remaining`
- Spawn validation: `road_or_playable_ground_only_spawn_validated_3_attempts_2_building_rejections_nearest_building_4_70m`
- Lighting: `round2_day_preserved_night_sky_dark_buildings_readable_by_profile_and_smoke_manual_confirmation_remaining`
- Debug cleanup: `production_mode_debug_objects_hidden_by_preflight_and_player_log_manual_visual_confirmation_remaining`
- Material/LOD visual: `round2_material_texture_audit_documented_no_lod2_claim_manual_acceptance_remaining`
- Ground alignment: `round3_support_road_building_reference_delta_0_00m_spawn_y_0_04m_player_smoke_passed`
- Building clipping: `round3_building_road_y_offset_1_97m_corrected_runtime_manual_visual_confirmation_remaining`
- Playable bounds: `runtime_invisible_air_walls_4_colliders_0_renderers_player_smoke_passed`
- Name labels: `offline_only_93_available_8_active_0_fabricated_road_or_building_names_player_smoke_passed`
- NPC distribution: `validated_160_requested_160_spawned_within_1000m_24_sectors_5_rings_no_cap`
- Gameplay regression: `EditMode_114_114_PlayMode_27_27_passed`
- Temp player: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapGroundVisualRound3Pre\ChuoTsunamiEvacuation_NewMapGroundVisualRound3Pre.exe`
- Player.log: 0 errors, 0 warnings; spawn smoke passed; left/right drag smoke passed; name-label smoke passed
- Ground smoke: support 0.00m, road sample 0.00m, corrected building base 0.00m, spawn Y 0.04m, support-to-road delta 0.00m, support-to-building-base delta 0.00m, 8 active target height-offset violations documented for manual classification
- Support visibility smoke: support collider active, support renderer count 0, visible support renderers 0
- Air wall smoke: 4 colliders, 0 visible renderers
- Name label smoke: 93 labels available, 8 active, 15 official shelter labels, 78 non-official candidate labels, 0 road/building labels because no source name is available
- Night smoke: sky brightness 0.042, building readability score 0.517, day restored
- FPS/stutter sample: 180s, 160 NPCs active, 3324.68 average FPS in hidden smoke run, 2 frames over 66ms
- EditMode: passed 115/115
- PlayMode: passed 28/28
- Preflight: passed
- DeepSeek: passed, no A-level blockers; ready with documented visual limitations

Remaining limitations:

- LOD2 textured building quality is not claimed because the documented current import is Buildings/LOD1.
- Photo-projected/appearance texture roughness is documented as an import/PLATEAU material limitation unless a later approved material conversion or reimport proves otherwise.
- A single flat gameplay support plane cannot perfectly match every elevated terrain/building base across the full Chuo map; 2 active target anchors remain more than 2.5m from the support height and require manual visual classification.
- Physical mouse drag feel and cursor behavior still need confirmation in the manual build.
- Manual visual confirmation is still required for night building readability and ground/building alignment from the player camera.
- Manual spawn confirmation is required to verify the player starts on visible road/playable ground and can move immediately.
- Manual confirmation is required that the blue support/collision surface is gone.
- Manual confirmation is required that invisible air walls block map-edge exit without visible walls.
- Generic road/building labels remain hidden unless source metadata or confidence-gated local cache provides real names.
- Building Y correction is a uniform runtime offset from sampled transport ground to sampled building bases; per-building terrain variation still requires manual visual review.
- Building detection relies on imported `bldg_`/building naming; any unnamed building meshes may need a later manual override list if visible floating remains.
