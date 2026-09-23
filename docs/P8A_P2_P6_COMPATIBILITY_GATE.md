# P8-A P2-P6 Compatibility Gate

Date: 2026-05-23.

## Gate Status

P8-A creates a compatibility gate for the P7 practical baseline. It does not change gameplay success/failure rules.

## P2

- Player movement: existing `SimplePlayerController` remains available for scene smoke testing.
- Camera compatibility: existing gameplay camera/controller assumptions remain unchanged by P8-A.
- Shelter interaction: existing shelter trigger scripts remain available.
- Result panel and success/failure flow: `ResultPanelController` and result data remain untouched.

## P3

- Data pipeline outputs remain consumed from `Assets/Data` runtime copies.
- P8-A adds isolated `Assets/Data/P8` JSON only.
- Existing P3 runtime data assumptions are not changed.

## P4

- Real shelter loading and marker placement remain available through existing loaders/generators.
- Marker placement may need P8 scene smoke testing against the high-detail map.

## P5

- Qualified shelters, routes, high-rise candidates, and humanitarian candidate display logic remain opt-in and data-driven.
- `real_qualified` remains fail-safe and opt-in.
- No unsafe official-route claim is introduced by P8-A.

## P6

- Navigation guidance and NPC prototype scripts are not hard-bound to `Chuo_BaseMap`.
- New map target smoke testing remains a P8 compatibility follow-up.
- P8-A does not add P9 crowd or indoor evacuation behavior.

## Required Follow-Up

Run a new-map smoke test on `P7_HighDetail_Chuo.unity` before P8-B visual work becomes scene-dependent.
