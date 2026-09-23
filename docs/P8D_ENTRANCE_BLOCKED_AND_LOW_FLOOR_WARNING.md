# P8-D Entrance Blocked And Low-Floor Warning

Date: 2026-05-24.

## Entrance Blocked Proxy

`entrance_blocked_proxy` is emitted when an entrance proxy has local inundation depth or hazard intensity above configured thresholds.

It means the entrance should be visually warned/restricted in a future scene. It does not mean the real entrance is officially closed, and it does not change player win/loss rules in P8-D.

## Low-Floor Inundation Warning

`low_floor_inundation_warning` is emitted when a building-like target has inundation depth above the low-floor threshold but below the stronger building-damage proxy threshold.

This is a proxy warning for future vertical evacuation presentation. It is not real indoor simulation and does not require a BIM/LOD4 interior.

## Future Use

P8-E should verify persistent visibility and handoff instructions. P9 may later use entrance / safe-floor / evacuation-complete proxies if the user approves selectable life-first vertical evacuation targets.
