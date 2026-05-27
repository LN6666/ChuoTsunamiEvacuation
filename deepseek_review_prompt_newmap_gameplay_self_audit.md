# DeepSeek Review Prompt - NewMap Gameplay Self-Audit

Generated: 2026-05-27T20:14:37+09:00

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Verify:
- actual gameplay reachability is checked, not only scripts/data
- official shelter gameplay flow exists
- non-official candidate gameplay flow exists with warnings
- P9 outcome scenarios work
- Tourism/Evacuation modes work
- route wording is honest and no official route claim exists
- disabled targets are inactive
- non-official warnings are preserved
- no vague completion statuses are used
- no final release/archive was created
- no P10-E/F/G was created
- no A-level blockers remain

Important files:
- `Assets/Data/P10/newmap_gameplay_self_audit_matrix.json`
- `Assets/Data/P10/newmap_official_shelter_gameplay_check.json`
- `Assets/Data/P10/newmap_non_official_candidate_gameplay_check.json`
- `Assets/Data/P10/newmap_route_gameplay_check.json`
- `Assets/Data/P10/newmap_mode_gameplay_check.json`
- `Assets/Data/P10/newmap_interaction_flow_check.json`
- `Assets/Data/P10/newmap_outcome_scenario_check.json`
- `Assets/Data/P10/newmap_ui_ux_runtime_check.json`
- `Assets/Data/P10/newmap_gameplay_self_audit_player_report.json`
- `Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs`
- `Assets/Scripts/NewMap/NewMapGameController.cs`
- `Assets/Tests/PlayMode/NewMapRuntimePlayModeTests.cs`
