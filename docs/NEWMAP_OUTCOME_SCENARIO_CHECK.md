# NewMap Outcome Scenario Check

Generated: 2026-05-27T20:14:37+09:00

| scenarioId | mode | targetId | expectedOutcome | actualOutcome | reasonCode | passFail |
|---|---|---|---|---|---|---|
| success_official_shelter | Evacuation | first active official shelter | safe_floor_reached | passed | safe_floor_reached | pass |
| success_non_official_candidate_with_warning | Evacuation | p8_plateau_highrise_candidate_001 | safe_floor_reached with warning | passed | safe_floor_reached | pass |
| crowd_delay_success_or_failure | Evacuation | newmap_proxy_crowd_delay | crowd delay detail appears | passed | Entering shelter proxy | pass |
| entrance_blocked_failure | Evacuation | newmap_proxy_blocked_entrance | blocked entrance failure | passed | entrance_blocked | pass |
| safe_floor_unavailable_failure | Evacuation | newmap_proxy_no_safe_floor | no safe floor failure | passed | safe_floor_unavailable | pass |
| collapse_debris_exposure_failure | Evacuation | Stage 2 debris proxy | collapse/debris failure | passed | collapse_debris_exposure | pass |
| collapse_disabled_success | Tourism | Stage 2 debris proxy | Tourism ignores collapse/debris | passed | no failure | pass |
| tsunami_front_failure | Evacuation | Stage 2 risk front | risk front failure | passed | tsunami_front_contact | pass |
| tourism_free_roam_no_failure | Tourism | runtime target set | Tourism has no hazard/crowd/collapse failure | passed | Tourism inspection | pass |
| disabled_target_not_selectable | Evacuation | disabled_out_of_new_map_candidate_probe | not selectable | passed | not selectable | pass |
