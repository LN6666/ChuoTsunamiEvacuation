# Codex Prompt: NewMap Full P2-P10 Integration

Use `D:\UnityProjects\ChuoTsunamiEvacuation` as the main workspace.

The active scene is `Assets/Scenes/Chuo_BaseMap.unity`.

Do not use old P7_HighDetail, P10RealMap, or LowSpec failed scene outputs as active targets.

Do not claim old P3/P4/P5 targets are active unless they have verified anchors in the reset map.

Validate:

- `tools/map/run_newmap_full_integration_preflight.ps1`
- EditMode Unity tests.
- PlayMode Unity tests.
- Temporary build and performance sampling if feasible.
- DeepSeek review.

Use strict completion statuses only:

- `completed_on_new_chuo_basemap`
- `completed_with_documented_runtime_proxy`
- `disabled_missing_from_new_map`
- `blocked_needs_user_asset`
- `failed`
