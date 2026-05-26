# DeepSeek Review Prompt - NewMap Official Shelter / Route / Spike Hardening

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Verify:
- official shelters are either genuinely anchored by exact PLATEAU GML object names in `Assets/Scenes/Chuo_BaseMap.unity` or honestly disabled
- P3/P4 recovery is not faked
- route geometry validation is honest and old WGS84 routes are not claimed active on the new map
- no official route false claim exists
- disabled targets are inactive
- startup spike reduction changes are meaningful and do not remove required gameplay
- memory is recorded honestly but not treated as the main task focus
- Tourism and Evacuation modes still work
- P9 scenario outcomes still work
- no final release/archive was created
- no P10-E/F/G was created
- no A-level blockers remain

Important reports:
- `Assets/Data/P10/newmap_official_shelter_anchor_report.json`
- `Assets/Data/P10/newmap_coordinate_transform_validation.json`
- `Assets/Data/P10/newmap_route_geometry_validation.json`
- `Assets/Data/P10/newmap_spike_reduction_report.json`
- `Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json`
