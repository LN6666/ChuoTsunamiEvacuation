# DeepSeek Review Prompt: NewMap Completion Hardening

Review the git diff for `D:\UnityProjects\ChuoTsunamiEvacuation` on branch `phase5-qualification-routing-plateau`.

Active scene must remain:

`Assets/Scenes/Chuo_BaseMap.unity`

Check the hardening work for these requirements:

- No vague completion language in docs or JSON.
- `Assets/Scenes/Chuo_BaseMap.unity` is the active runtime/build target.
- P2-P10 completion matrix uses only:
  - `completed_on_new_chuo_basemap`
  - `completed_with_documented_runtime_proxy`
  - `disabled_missing_from_new_map`
  - `blocked_needs_user_map_asset`
  - `failed`
- Disabled targets are inactive, unselectable, hidden from green frames/routes, and excluded from ResultPanel success.
- At least one complete active target flow works.
- P3/P4/P5 are not falsely claimed if official shelters/routes are missing from the new map.
- Tourism Mode and Evacuation Mode work.
- P9 scenario outcomes work and have reason-code evidence.
- Performance measurement is meaningful and memory/spike limitations are documented.
- No P10-E/F/G files were created.
- No release/archive package was created.
- No official route false claim.
- No non-official safe approval claim.
- No A-level blockers remain.

Focus on compile risks, NullReferenceException risks, Unity lifecycle issues, false status claims, and missing tests.
