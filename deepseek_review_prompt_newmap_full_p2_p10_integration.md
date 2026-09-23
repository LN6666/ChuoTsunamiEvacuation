# DeepSeek Review Prompt: NewMap Full P2-P10 Integration

Review the git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Focus:

- The new `Assets/Scenes/Chuo_BaseMap.unity` is the active baseline.
- No old P7_HighDetail, P10RealMap, or LowSpec failed scene is an active target.
- P2-P10 systems are either connected to the new map or honestly disabled/blocked.
- Missing/out-of-map targets are disabled/commented and not spawned.
- Tourism Mode and Evacuation Mode work as documented.
- Stage 1 Warning hides the light curtain and ignores risk contact.
- Stage 2 FrontApproaching shows the curtain and enables hazard checks.
- Player/camera/UI/ResultPanel work.
- Runtime local proxies are clearly documented and not falsely official.
- P3/P4/P5 official/qualified/route targets are not falsely active when no new-map anchor exists.
- Routes are never claimed as official routes.
- Non-official candidates are not safe-approved by default.
- Green frames do not imply official safety approval.
- P6/P8/P9 proxy systems have clear runtime behavior and reason codes.
- Performance validation is meaningful and does not fake a pass.
- No final release/archive was created.
- No P10-E/F/G was created.

Key files:

- `Assets/Scripts/NewMap/`
- `Assets/Scripts/Editor/NewMapSceneSetupUtility.cs`
- `Assets/Data/P10/*.json`
- `docs/NEWMAP_*.md`
- `docs/GAME_RULES_EN.md`
- `docs/GAME_RULES_JA.md`
- `tools/map/*.ps1`

Please report:

- A-level blockers first.
- Compile risks.
- NullReference risks.
- Unity lifecycle risks.
- Scene-size/performance risks.
- Any false claim of official routes, official safety, or old target activity.
- Whether the completion matrix statuses are honest.
