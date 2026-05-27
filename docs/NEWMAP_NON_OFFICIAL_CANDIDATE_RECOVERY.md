# NewMap Non-Official Candidate Recovery

Generated: 2026-05-27T19:21:33+09:00

Final status: `completed_with_documented_runtime_proxy`

- Candidate records loaded: 110
- Active exact GML anchors: 61
- Active nearest-building anchors: 3
- Active coordinate-proxy anchors: 14
- Final active non-official humanitarian candidates: 78
- Disabled outside Chuo_BaseMap: 32
- Disabled missing coordinate: 0
- Disabled low confidence: 0

The 4-target non-official count is no longer treated as sufficient. The P8/P9 110-record handoff was reprocessed with the validated official-anchor transform. ID-only candidates were accepted when their coordinates transformed inside the new map and the non-official warning remained mandatory.

Large inactive group: 32 records transform outside the Chuo_BaseMap mesh AABB and remain disabled; no record is disabled merely for being ID-only.

All recovered candidates remain `isOfficialShelter=false`, `nonOfficialWarningRequired=true`, and `safeApprovedByDefault=false`.
