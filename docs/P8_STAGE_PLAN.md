# P8 Stage Plan

P8 has exactly four stages.

## P8-A

Baseline handoff from P7, P2-P6 compatibility gate, and tsunami hazard data layer foundation.

Deliverables:

- Preserve `P7_HighDetail_Chuo.unity` as the practical baseline.
- Confirm P2-P6 compatibility assumptions against the new baseline.
- Add hazard data schema, sample data, visualization config, infrastructure interaction config, loader, validator, tests, and preflight.
- Keep scientific hazard data separate from cinematic visualization parameters.

## P8-B

Dynamic tsunami light curtain and risk front visualization.

Deliverables:

- Read P8-A hazard data/config.
- Implement visual risk-front progression without claiming physical fluid simulation.
- Keep cinematic height marked as visual/gameplay representation.

## P8-C

Road, building, bridge, and underground hazard interaction.

Deliverables:

- Connect hazard layers to infrastructure categories where P7 scene evidence exists.
- Use proxy, marker, or rule-based fallbacks where detailed geometry is missing.
- Keep gameplay success/failure rules unchanged unless a later approved stage explicitly changes them.

## P8-D

Infrastructure hazard interaction, lightweight building damage/collapse proxy, and P8 closeout.

Deliverables:

- Add data-driven damage/collapse proxy behavior.
- Validate performance, tests, scene compatibility, and handoff to P9.
- Close P8 without implementing P9 crowd, real spawn, indoor shelter, congestion, or P10 packaging systems.
