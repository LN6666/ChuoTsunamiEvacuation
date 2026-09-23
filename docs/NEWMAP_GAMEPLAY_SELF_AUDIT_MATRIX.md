# NewMap Gameplay Self-Audit Matrix

Generated: 2026-05-27T20:14:37+09:00

| featureId | phase | runtimeReachable | playerBuildEvidence | finalStatus | remainingLimitation |
|---|---|---|---|---|---|
| p2_player_spawn | P2 | True | True | `completed_with_documented_runtime_proxy` | Ground support proxy remains documented when scene collider grounding is not reliable. |
| p2_player_visible | P2 | True | True | `completed_on_new_chuo_basemap` |  |
| p2_camera_visible | P2 | True | True | `completed_on_new_chuo_basemap` |  |
| p2_movement | P2 | True | True | `completed_with_documented_runtime_proxy` | Automated movement uses diagnostic movement helper; manual build uses WASD. |
| p2_sprint | P2 | True | True | `completed_on_new_chuo_basemap` |  |
| p2_stamina | P2 | True | True | `completed_on_new_chuo_basemap` |  |
| p2_e_interaction | P2 | True | True | `completed_with_documented_runtime_proxy` | Automated player-build smoke uses deterministic diagnostic interaction; manual build still exposes E interaction. |
| p2_result_panel | P2 | True | True | `completed_on_new_chuo_basemap` |  |
| p3p4_official_dataset_loading | P3/P4 | True | True | `completed_on_new_chuo_basemap` |  |
| p3p4_official_markers | P3/P4 | True | True | `completed_on_new_chuo_basemap` |  |
| p3p4_official_interaction | P3/P4 | True | True | `completed_with_documented_runtime_proxy` | Safe-floor timing is prototype gameplay, not official building safety certification. |
| p3p4_official_result_flow | P3/P4 | True | True | `completed_with_documented_runtime_proxy` |  |
| p5_route_guidance_display | P5 | True | True | `completed_with_documented_runtime_proxy` | Runtime route display is local target guidance; old route geometry is report evidence only. |
| p5_estimated_route_wording | P5 | True | True | `completed_with_documented_runtime_proxy` |  |
| p5_route_target_selection | P5 | True | True | `completed_with_documented_runtime_proxy` |  |
| p5_disabled_route_suppression | P5 | True | True | `completed_on_new_chuo_basemap` |  |
| p5_no_official_route_claim | P5 | True | True | `completed_on_new_chuo_basemap` |  |
| p6_npc_spawn | P6 | True | True | `completed_with_documented_runtime_proxy` | Road-aware navigation remains unavailable. |
| p6_npc_humanoid_appearance | P6 | True | True | `completed_with_documented_runtime_proxy` |  |
| p6_npc_proxy_movement | P6 | True | True | `completed_with_documented_runtime_proxy` | This is local proxy movement, not full crowd simulation. |
| p6_crowd_delay | P6 | True | True | `completed_with_documented_runtime_proxy` |  |
| p6_crowd_metrics | P6 | True | True | `completed_with_documented_runtime_proxy` |  |
| p6_tourism_no_crowd_failure | P6 | True | True | `completed_on_new_chuo_basemap` |  |
| p8_two_stage_tsunami_warning | P8 | True | True | `completed_with_documented_runtime_proxy` |  |
| p8_stage1_warning_ui | P8 | True | True | `completed_with_documented_runtime_proxy` |  |
| p8_stage2_light_curtain | P8 | True | True | `completed_with_documented_runtime_proxy` |  |
| p8_hazard_activation_timing | P8 | True | True | `completed_with_documented_runtime_proxy` |  |
| p8_no_instant_death | P8 | True | True | `completed_on_new_chuo_basemap` |  |
| p8_tourism_disables_tsunami | P8 | True | True | `completed_on_new_chuo_basemap` |  |
| p9_weighted_spawn | P9 | True | True | `completed_with_documented_runtime_proxy` | Weighted GIS spawn selection is disabled_missing_from_new_map until user supplies verified NewMap spawn anchors. |
| p9_entrance_interaction | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p9_safe_floor_vertical_proxy | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p9_crowd_delay_outcome | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p9_entrance_blocked_failure | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p9_safe_floor_unavailable_failure | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p9_collapse_debris_exposure | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p9_collapse_disabled_success | P9 | True | True | `completed_on_new_chuo_basemap` |  |
| p9_tsunami_front_failure | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p9_reason_codes | P9 | True | True | `completed_with_documented_runtime_proxy` |  |
| p10_start_menu | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_mode_selection | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_localization | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_scrollable_rules | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_pause_menu | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_quit_force_quit | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_weather_night | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_green_ground_frames | P10 | True | True | `completed_with_documented_runtime_proxy` |  |
| p10_official_nonofficial_warnings | P10 | True | True | `completed_on_new_chuo_basemap` |  |
| p10_player_npc_humanoids | P10 | True | True | `completed_with_documented_runtime_proxy` |  |
| p10_performance_metrics_hooks | P10 | True | True | `completed_on_new_chuo_basemap` | Startup spike remains above 2000 ms unless player retest proves otherwise. |
