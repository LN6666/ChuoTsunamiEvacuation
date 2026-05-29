# P10 NewMap Tsunami Mode Hotfix Report

## Scope

This hotfix keeps the existing P10/New Chuo_BaseMap systems and addresses playable blockers in tsunami mode. It does not re-import PLATEAU data, does not add P10-E/F/G, and does not claim GIS-grade tsunami, route, terrain, or interior accuracy.

## Implemented Fixes

- Failure/result flow: tsunami-front, debris/collapse, blocked-entry, and no-safe-floor failures now leave the player locked and show the result panel with a visible `Retry / Restart` option.
- Restart/reset: retry returns to the start menu, clears failure/building-entry state, hides hazard visuals, resets UI state, and selects a fresh valid random spawn.
- Spawn: normal sessions use randomized valid playable-ground spawn candidates before the old map-center fallback. Deterministic seed override remains available for tests.
- Air walls: generated boundary blockers are clearly named `P10_BoundaryAirWall_North/South/East/West`; unknown playable-area blockers are converted to triggers instead of remaining invisible walls.
- Stamina: evacuation stamina baseline remains `100`, multiplier is `100`, final max stamina is `10000`.
- Tsunami origin: the configured coastal side is `south`, with the front moving inland along `Vector3.forward`.
- Two-stage tsunami: warning phase is config-driven, defaults to exactly `300s`, and remains separate from active front movement/failure checks.
- Light curtain: configured at `1000m` high and at least `map diagonal * 1.5` long, with a minimum of `1500m`.
- Building entry: eligible targets get `P10_BuildingEntryTrigger_<id>` trigger volumes. Verified shelters use building renderer bounds where available, so touching/overlapping the eligible building volume is sufficient; no door, raycast, or precise marker aim is required.
- Vertical evacuation: if no real interior exists, the existing safe-floor proxy flow is used and documented as a proxy, not an interior scene.

## Validation

- EditMode: passed, `123/123`.
- PlayMode: passed, `39/39`.
- Existing NewMap runtime preflight: passed.
- P10 tsunami-mode hotfix preflight: passed.
- Temporary player build: passed.
- Player.log parse: passed, `0` errors and `0` warnings.
- Player-build smoke: `tsunami_warning_300s_before_active` passed; active tsunami started after diagnostic fast-forward to `301s`.
- Player-build smoke: `building_touch_e_entry` passed on `chuo_official_emergency_001`; no precise entrance marker was used.
- DeepSeek review: unavailable; normal and escalated retries failed with `APIConnectionError` / remote server disconnect and timeout. The timed-out Python process was stopped.

## Current Runtime Values

- Stamina max: `10000`.
- Warning duration: `300s`.
- Tsunami start side: `south`.
- Tsunami direction: `Vector3.forward`.
- Curtain height: `1000m`.
- Curtain minimum length: `1500m`; runtime length is at least playable-map diagonal times `1.5`.
- Runtime curtain length from player smoke: `8208.7m`.
- Boundary air walls: `4`, named `P10_BoundaryAirWall_North`, `P10_BoundaryAirWall_South`, `P10_BoundaryAirWall_East`, `P10_BoundaryAirWall_West`.
- Building entry triggers in player smoke: `97`; physical building-entry blockers: `0`.
- Player smoke spawn: source `random_playable_support`, deterministic seed disabled.
- NPC lifecycle regression smoke: global respawns `0`, all-stop events `0`, stopped-without-reason `0`.

## Critical Blocker Follow-Up

- Warning blocker cause: the real P10 config still used a `20s` warning. It now records `tsunamiWarningDurationSeconds: 300.0` and the legacy `warningPhaseSeconds: 300.0` for compatibility.
- Test override: `NewMapTsunamiModeHotfixConfig.CreateForDiagnostics(seconds)` supports short automated durations without changing the real gameplay config.
- E-entry blocker cause: previous trigger volumes were small target-anchor boxes, so touching the actual building facade could miss the enterable volume.
- E-entry fix: verified shelters now create `P10_BuildingEntryTrigger_<id>` from the building renderer bounds when available; candidates use nearest building bounds or target proxy fallback. These are trigger-only `BoxCollider` volumes and do not physically block the player.
- Interior status: entry uses the existing vertical evacuation safe-floor proxy. No real interior scene is claimed.

## Remaining Limitations

- Tsunami is still a gameplay risk-front/light-curtain representation, not fluid simulation.
- Building entry uses vertical evacuation proxy state/safe-floor timing unless real interiors are later integrated.
- Spawn points are validated against current gameplay ground/support and building bounds, not official road/network data.
