# NewMap Manual Playtest Readiness

Generated: 2026-05-29T00:00:00+09:00

Decision: `ready_with_documented_visual_limitations`

Reason: preflight, EditMode, PlayMode, temp player build, fresh Player.log parse, and DeepSeek review passed. Manual-visible confirmation is still required for final brightness, material quality, ground feel, and imported-geometry clipping.

- Mouse look: `automated_state_transition_passed_physical_mouse_manual_confirmation_remaining`
- Lighting: `clear_day_and_weather_lighting_configured_tests_passed_manual_brightness_confirmation_remaining`
- Debug cleanup: `production_mode_debug_objects_hidden_by_tests_and_preflight_manual_visual_confirmation_remaining`
- Material/LOD visual: `no_magenta_shader_expected_lighting_shader_fallback_documented_lod1_limitation`
- Ground alignment: `player_spawn_support_delta_0_04m_player_build_log_clean_manual_visible_confirmation_remaining`
- Building clipping: `runtime_support_proxy_clipping_fixed_imported_geometry_limitations_manual_confirmation_remaining`
- NPC distribution: `validated_160_requested_160_spawned_within_1000m_24_sectors_5_rings_no_cap`
- Gameplay regression: `EditMode_113_113_PlayMode_26_26_passed`
- Temp player: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapVisualFixPre\ChuoTsunamiEvacuation_NewMapVisualFixPre.exe`
- Player.log: 0 errors, 0 warnings
- FPS/stutter sample: 180s, 160 NPCs active, 3598.53 average FPS in hidden smoke run, 2 frames over 66ms
- EditMode: passed 113/113
- PlayMode: passed 26/26
- Preflight: passed
- DeepSeek: passed, no A-level blockers

Remaining limitations:

- LOD2 textured building quality is not claimed because the documented current import is Buildings/LOD1.
- Any PLATEAU source geometry clipping still visible after runtime support/proxy cleanup requires manual classification.
- Automated tests cannot replace direct visual confirmation of brightness, material appearance, support alignment, and clipping in the manual camera view.
- NPC config is copied into the NewMapVisualFixPre player data folder by the temp build script; other player builds need the same copy/streaming step for external config tuning.
