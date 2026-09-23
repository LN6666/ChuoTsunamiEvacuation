# P10 NewMap Tsunami Mode Hotfix Report

## Scope

This hotfix keeps the existing P10/New Chuo_BaseMap systems and addresses playable blockers in tsunami mode. It does not re-import PLATEAU data, does not add P10-E/F/G, and does not claim GIS-grade tsunami, route, terrain, or interior accuracy.

## Implemented Fixes

- Failure/result flow: tsunami-front, debris/collapse, blocked-entry, and no-safe-floor failures now leave the player locked and show the result panel with a visible `Retry / Restart` option.
- Restart/reset: retry returns to the start menu, clears failure/building-entry state, hides hazard visuals, resets UI state, and selects a fresh valid random spawn.
- Spawn: normal sessions use randomized valid playable-ground spawn candidates before the old map-center fallback. Deterministic seed override remains available for tests.
- Air walls: old rectangular `P10_BoundaryAirWall_*` blockers are no longer created for normal movement. Map containment now uses the invisible 2.27km circular runtime clamp; unknown playable-area blockers are converted to triggers or disabled instead of remaining invisible walls.
- Stamina: evacuation stamina baseline remains `100`; multiplier changed from `100` to `200`, so final max stamina changed from `10000` to `20000`.
- Sprint: evacuation sprint speed uses `sprintSpeedMultiplierAdditional: 1.35`, changing clear-weather evacuation sprint from `5.0m/s` to `6.75m/s`. Walking speed remains `1.0m/s`.
- Tsunami origin: the configured coastal side is `south`, with the front moving inland along `Vector3.forward`.
- Three-phase tsunami mode: evacuation now starts in `PRE_WARNING_WAIT`, waits a per-session random `0-180s`, then enters `WARNING` for exactly `300s`, then enters active tsunami/front movement. Hazard failure and light curtain visuals remain inactive before active tsunami.
- Light curtain: configured at `1000m` high and at least `map diagonal * 1.5` long, with a minimum of `1500m`.
- Building entry: eligible targets get `P10_BuildingEntryTrigger_<id>` trigger volumes. Verified shelters use building renderer bounds where available, so touching/overlapping the eligible building volume is sufficient; no door, raycast, or precise marker aim is required.
- Vertical evacuation: if no real interior exists, the existing safe-floor proxy flow is used and documented as a proxy, not an interior scene.
- Shelter direct-line guidance: `P10_ShelterDirectLineController` creates straight player-to-target `LineRenderer` guidance for every rankable active target. It does not calculate road-network routes and creates no colliders.
- Direct-line colors: nearest target is red; non-nearest official targets are green; non-nearest non-official/humanitarian/proxy targets are yellow. The red nearest override is recomputed dynamically as the player moves.
- Ranking UI: pressing `R` shows/refreshes one mixed straight-line shelter ranking. While visible, it auto-refreshes every `0.5s`. The ranking does not split official and non-official targets.

## Validation

- EditMode: passed, `123/123`.
- PlayMode: passed, `41/41`.
- Existing NewMap runtime preflight: passed.
- P10 tsunami-mode hotfix preflight: passed.
- Temporary player build: passed.
- Player.log parse: passed, `0` errors and `0` warnings.
- Player-build smoke: `tsunami_warning_300s_before_active` passed; active tsunami started after diagnostic fast-forward to `301s`.
- Player-build smoke: `building_touch_e_entry` passed on `chuo_official_emergency_001`; no precise entrance marker was used.
- DeepSeek review: unavailable; normal and escalated retries failed with `APIConnectionError` / remote server disconnect and timeout. The timed-out Python process was stopped.

## Current Runtime Values

- Stamina max: `20000`.
- Previous stamina max: `10000`.
- Evacuation sprint speed: `6.75m/s` clear weather, from previous `5.0m/s`.
- Sprint multiplier: `1.35`.
- Pre-warning random range: `0-180s`.
- Warning duration: `300s`.
- Tsunami start side: `south`.
- Tsunami direction: `Vector3.forward`.
- Curtain height: `1000m`.
- Curtain minimum length: `1500m`; runtime length is at least playable-map diagonal times `1.5`.
- Runtime curtain length from player smoke: `8208.7m`.
- Boundary: 2.27km circular runtime clamp from the original map center. Old rectangular boundary air walls: `0` active.
- Building entry triggers in player smoke: `97`; physical building-entry blockers: `0`.
- Shelter direct-line normal Chuo_BaseMap target count: `93` rankable targets when the 15 verified official anchors are present (`15 official + 78 non-official`). Diagnostic PlayMode without full map anchors generated `80`; with one official-anchor fixture generated `81`.
- Direct-line config path: `Assets/Data/P10/newmap_tsunami_mode_hotfix_config.json`.
- Direct-line config values: `enableShelterDirectLines: true`, `maxDisplayedShelterLines: 0` (`0` means all), `lineUpdateIntervalSeconds: 0.2`, `rankingAutoRefreshIntervalSeconds: 0.5`, `directLineHeightOffsetMeters: 0.35`, `directLineWidthMeters: 0.08`.
- Player movement config path: `Assets/Data/P10/newmap_player_stamina_config.json`.
- Player movement config values: `baselineMaxStamina: 100`, `staminaMultiplier: 200`, `sprintSpeedMultiplierAdditional: 1.35`.
- Player smoke spawn: source `random_playable_support`, deterministic seed disabled.
- NPC lifecycle regression smoke: global respawns `0`, all-stop events `0`, stopped-without-reason `0`.

## Critical Blocker Follow-Up

- Warning blocker cause: the real P10 config still used a `20s` warning. It now records `tsunamiWarningDurationSeconds: 300.0` and the legacy `warningPhaseSeconds: 300.0` for compatibility.
- Test override: `NewMapTsunamiModeHotfixConfig.CreateForDiagnostics(seconds)` supports short automated durations without changing the real gameplay config.
- Pre-warning override: `NewMapTsunamiModeHotfixConfig.CreateForDiagnostics(warningSeconds, preWarningSeconds)` supports deterministic pre-warning durations in tests. Normal gameplay keeps `preWarningTestOverrideSeconds: -1.0` and `deterministicPreWarningSeedEnabled: false`.
- E-entry blocker cause: previous trigger volumes were small target-anchor boxes, so touching the actual building facade could miss the enterable volume.
- E-entry fix: verified shelters now create `P10_BuildingEntryTrigger_<id>` from the building renderer bounds when available; candidates use nearest building bounds or target proxy fallback. These are trigger-only `BoxCollider` volumes and do not physically block the player.
- Interior status: entry uses the existing vertical evacuation safe-floor proxy. No real interior scene is claimed.

## Additional P10 Addendum Validation

- Direct shelter line creation: covered by `RuntimeShelterDirectLinesRankAndRecolorDynamically`; all rankable active targets get a line and line objects have `0` colliders.
- Line color ranking: covered by the same PlayMode test; nearest official/non-official targets turn red, and the previous nearest returns to green/yellow.
- Ranking UI: covered by `RuntimeShelterRankingUiRefreshesMixedStraightLineDistances`; `R` behavior is implemented through the same refresh path, mixed ranking text is shown, and visible ranking auto-refreshes.
- Pre-warning phase: covered by `RuntimeTsunamiWarningPrecedesCoastalHugeCurtain`; max is `180s`, warning lasts `300s`, hazard checks are inactive before active tsunami, and active start is scheduled at `warning_start_time + 300s`.
- Regression: existing failure/retry, random spawn, airwall cleanup, coastal tsunami side, building-touch `E` entry, and no-collider direct-line behavior are covered by the updated PlayMode suite.

## Remaining Limitations

- Tsunami is still a gameplay risk-front/light-curtain representation, not fluid simulation.
- Shelter direct lines are straight-line gameplay guidance only. They are not official evacuation routes and do not use road/path distance.
- Ranking is based on currently loaded active/rankable runtime targets. Blocked or no-safe-floor diagnostic targets are excluded from the usable ranking instead of being shown as usable shelters.
- Building entry uses vertical evacuation proxy state/safe-floor timing unless real interiors are later integrated.
- Spawn points are validated against current gameplay ground/support and building bounds, not official road/network data.
