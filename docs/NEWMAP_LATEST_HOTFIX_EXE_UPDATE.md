# NewMap Latest Hotfix EXE Update

Generated at: 2026-05-30T05:18:06+09:00

## Scope

This is a temporary Windows player update for user testing of the latest P10 NewMap tsunami/stamina/shelter guidance hotfix. It is not a final release, archive, P11 move, or P10-E/F/G subphase.

## Git State

- Workspace: `D:\UnityProjects\ChuoTsunamiEvacuation`
- Branch: `phase5-qualification-routing-plateau`
- Build commit: `fa572d1c7e443e935e36430aac35cd991a220b3f`
- Commit message: `P10 shelter lines and prewarning tuning`
- Branch tracking: `origin/phase5-qualification-routing-plateau`, no ahead/behind marker before this local report/tooling update
- Worktree: not clean. Existing unrelated Unity-generated scene/settings/init-scene changes remain local and were not staged.

## Build

- Active scene: `Assets/Scenes/Chuo_BaseMap.unity`
- EXE path: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapLatestHotfixPre\ChuoTsunamiEvacuation_NewMapLatestHotfixPre.exe`
- Build result: passed
- EXE timestamp: 2026-05-30 05:14:06 +09:00
- EXE size: 667648 bytes
- Runtime config copy: passed. Latest P10 config JSON files were copied to `ChuoTsunamiEvacuation_NewMapLatestHotfixPre_Data\Data\P10`.

## Validation

- Latest EXE update preflight: passed, 36 checks
- P10 tsunami-mode hotfix preflight: passed
- Runtime no-web-request check: passed
- No false official-route-claim check: passed
- Unexpected airwall cleanup check: passed
- EditMode GUI tests: passed, 123/123
- PlayMode GUI tests: passed, 41/41

## Runtime Smoke

- Player log: `Logs\newmap_latest_hotfix_pre_player.log`
- EXE launch smoke: passed with `-newmapSelfAuditSmoke`
- Self-audit completion: passed
- Start menu visible: passed
- Tourism Mode selectable/no failure: passed
- Evacuation Mode starts tsunami timing flow: passed
- Player/camera/UI bootstrap: passed via runtime bootstrap and UI smoke
- Player.log errors: 0
- Player.log warnings: 0
- Player.log exceptions: 0
- Missing asset/config count: 0
- Runtime web request lines: 0

## Hotfix Config Included

- `Assets/Data/P10/newmap_tsunami_mode_hotfix_config.json`
- `preWarningRandomMaxSeconds`: 180.0
- `tsunamiWarningDurationSeconds`: 300.0
- `warningPhaseSeconds`: 300.0
- `tsunamiStartSide`: `south`
- `enableShelterDirectLines`: true
- `maxDisplayedShelterLines`: 0, meaning all eligible lines
- `lineUpdateIntervalSeconds`: 0.2
- `rankingAutoRefreshIntervalSeconds`: 0.5
- `directLineWidthMeters`: 0.08

Runtime smoke observed:

- `pre_warning_random_duration_seconds=124.1`
- `warning_start_time=124.1`
- `warning_duration_seconds=300.0`
- `tsunami_active_start_time=424.1`
- `phase=ACTIVE_TSUNAMI`
- `hazardChecksActive=false` before active tsunami and `true` after active tsunami

## Stamina Config Included

- `Assets/Data/P10/newmap_player_stamina_config.json`
- `baselineMaxStamina`: 100.0
- `staminaMultiplier`: 200.0
- Final max stamina: 20000
- `sprintSpeedMultiplierAdditional`: 1.35
- Final evacuation sprint speed: 6.75

## Shelter Guidance Included

- Shelter direct-line controller: included
- Runtime line count: 95
- Rankable targets: 95
- Runtime target count: 15 official, 82 non-official/proxy
- Direct-line collider count: 0
- Blocking collider check: passed
- Nearest-red rule: verified by PlayMode tests
- Official/non-official ranking mix: verified by PlayMode tests
- No official evacuation route claim: passed

## Limitations

- Shelter guidance is straight-line visual gameplay guidance only.
- It is not road-network routing, official evacuation route guidance, GIS-grade route validation, or tsunami fluid simulation.
- Full 300-second warning timing is configured for gameplay; automated smoke advances diagnostics instead of waiting five real minutes.
- The local worktree still has unrelated Unity-generated files/changes that were not staged for this EXE update report.

## Manual Test Steps

1. Run `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapLatestHotfixPre\ChuoTsunamiEvacuation_NewMapLatestHotfixPre.exe`.
2. Confirm the Start Menu appears.
3. Start Tourism Mode and confirm tsunami/failure behavior is disabled.
4. Return to menu, start Evacuation Mode, and confirm shelter direct lines appear.
5. Press `R` to show/refresh the mixed shelter distance ranking.
6. Move the player and verify the nearest shelter line is red while other official lines are green and non-official/proxy lines are yellow.
7. Confirm the game starts in pre-warning wait, then a 300-second warning, then active tsunami.
