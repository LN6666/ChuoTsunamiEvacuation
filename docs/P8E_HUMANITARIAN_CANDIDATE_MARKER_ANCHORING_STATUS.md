# P8-E Humanitarian Candidate Marker Anchoring Status

The 110 humanitarian candidates are prepared for marker anchoring at data/coordinate level.

| marker mode | count | meaning |
|---|---:|---|
| `named_marker` | 28 | Building name and coordinate evidence are present. |
| `id_only_marker` | 82 | Coordinate evidence and fallback building ID are present, but no display name is available. |
| `cluster_marker` | 0 | No candidate currently needs clustered fallback. |
| `data_only_pending` | 0 | No candidate currently lacks coordinate readiness in the audit data. |

No final Unity scene anchors were added in this hardening pass. The practical baseline scene remains `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` and was not reset or staged.

P9 can use marker data for persistent visibility only with non-official warnings. P9 must not make these candidates selectable life-first targets until final gameplay rules are explicitly implemented.
