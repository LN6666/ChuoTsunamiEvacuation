# NewMap Ground Raise 30 Percent

## Scope

This pass raises gameplay ground cover/support only. Imported PLATEAU buildings, road/building meshes, UI, labels, route X/Z positions, and map center X/Z are not moved.

## Baseline

- Baseline mode: `existing_raise_offset`
- Previous sampled building-base raise: `3.0m`
- Previous final micro-raise: `0.3m`
- Old effective gameplay ground Y / raise offset: `3.3m`
- New effective gameplay ground Y / raise offset: `4.29m`
- Actual additional raise: `0.99m`
- Actual raise percent from chosen baseline: `30%`

The runtime reports these values through `newmap_ground_raise_30_report.json` and Player.log tokens prefixed with `groundRaise30`.

## Verification Targets

- Ground cover visual tiles and colliders share the raised Y.
- Runtime support uses the same raised ground Y.
- Player spawn, NPC placement, active targets, green frames, route markers, E zones, safe-floor proxy zones, and fall recovery points are resnapped to the raised support.
- Remaining building floating is measured but not claimed fully fixed.
- Blue/fall-through regression remains blocked.

This is not GIS-grade terrain or facade correction.
