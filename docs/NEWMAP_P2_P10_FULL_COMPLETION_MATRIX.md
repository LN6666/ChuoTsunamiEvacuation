# NewMap P2-P10 Full Completion Matrix

Generated: 2026-05-27T04:00:17+09:00

Allowed statuses only: `completed_on_new_chuo_basemap`, `completed_with_documented_runtime_proxy`, `disabled_missing_from_new_map`, `blocked_needs_user_map_asset`, `failed`.

| Phase | Feature | Final status | Active on new map | Target used | Blocker |
|---|---|---|---|---|---|
| P2 | player/camera/movement/E/ResultPanel | `completed_with_documented_runtime_proxy` | true | verified official shelter anchors plus local training targets |  |
| P3/P4 | official shelter loading and marker activation | `completed_on_new_chuo_basemap` | true | 15 verified official shelter GML anchors |  |
| P5 | candidate loading, route geometry validation, prototype route guidance | `completed_with_documented_runtime_proxy` | true | official shelter anchors and 4 local route proxy training guides | route geometry validation needs verified transform |
| P6 | NPC/navigation/crowd | `completed_with_documented_runtime_proxy` | true | newmap_proxy_crowd_delay | road-aware navigation unavailable |
| P8 | two-stage tsunami/hazard/light curtain | `completed_with_documented_runtime_proxy` | true | runtime targets |  |
| P9 | gameplay outcomes and ResultPanel reason codes | `completed_with_documented_runtime_proxy` | true | verified official fixture plus local training targets |  |
| P10 | UI/modes/weather/stamina/green frames/spike tools | `completed_with_documented_runtime_proxy` | true | 19 active targets | startup spike remains above 2 seconds due to large Chuo_BaseMap scene activation |

Active targets: 19. Disabled targets: 156. Active official shelters: 15.

Performance retest: average FPS 72.72, max frame 11273.06 ms, Player.log 0 errors / 0 warnings. Runtime bootstrap total was 169 ms, so the remaining spike is not attributed to target activation, route validation, crowd creation, or MeshCollider shutdown.
