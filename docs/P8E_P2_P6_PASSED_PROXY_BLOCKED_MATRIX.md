# P8-E P2-P6 Passed / Proxy / Blocked Matrix

The authoritative machine-readable matrix is `Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json`.

| system | item | status | P9 boundary |
|---|---|---|---|
| P2 | player movement | passed | P9 may tune final movement only. |
| P2 | camera | passed | P9 may adjust final framing. |
| P2 | shelter interaction | proxy_ready | P9 implements entrance/safe-floor/evacuation-complete behavior. |
| P2 | ResultPanel success-failure flow | passed | P8 does not change success/failure rules. |
| P3 | Unity-ready data assumptions | passed | P9 should consume P8 data instead of redoing extraction. |
| P4 | real shelter loading / markers | proxy_ready | P9 verifies final marker placement. |
| P5 | qualified shelters | proxy_ready | Official/non-official separation must remain. |
| P5 | routes | proxy_ready | Estimated guidance only, not official route. |
| P5 | humanitarian candidates | proxy_ready | P9 decides selectable life-first target behavior. |
| P5 | real_qualified mode | proxy_ready | P9 validates final real gameplay mode. |
| P5 | official route claim safety | passed | No official route claim without new evidence. |
| P6 | navigation guidance | proxy_ready | P9 implements final guidance UX. |
| P6 | NPC prototype | proxy_ready | P9 owns crowd/spawn/congestion. |
| P6 | behavior validation | p9_final_gameplay_required | Final behavior validation belongs to P9. |
